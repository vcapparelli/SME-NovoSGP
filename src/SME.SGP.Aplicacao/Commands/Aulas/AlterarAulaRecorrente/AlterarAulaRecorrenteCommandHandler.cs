﻿using MediatR;
using Sentry;
using SME.SGP.Aplicacao.Integracoes;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class AlterarAulaRecorrenteCommandHandler : IRequestHandler<AlterarAulaRecorrenteCommand, bool>
    {
        private readonly IMediator mediator;
        private readonly IRepositorioAula repositorioAula;
        private readonly IRepositorioNotificacaoAula repositorioNotificacaoAula;
        private readonly IServicoNotificacao servicoNotificacao;
        private readonly IUnitOfWork unitOfWork;

        public AlterarAulaRecorrenteCommandHandler(IMediator mediator,
                                                   IRepositorioAula repositorioAula,
                                                   IRepositorioNotificacaoAula repositorioNotificacaoAula,
                                                   IServicoNotificacao servicoNotificacao,
                                                   IUnitOfWork unitOfWork)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.repositorioAula = repositorioAula ?? throw new ArgumentNullException(nameof(repositorioAula));
            this.repositorioNotificacaoAula = repositorioNotificacaoAula ?? throw new ArgumentNullException(nameof(repositorioNotificacaoAula));
            this.servicoNotificacao = servicoNotificacao ?? throw new ArgumentNullException(nameof(servicoNotificacao));
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<bool> Handle(AlterarAulaRecorrenteCommand request, CancellationToken cancellationToken)
        {
            ValidarQuantidadeAulasCJ(request);
            await ValidarComponentesProfessor(request);

            var turma = await mediator.Send(new ObterTurmaComUeEDrePorCodigoQuery(request.CodigoTurma));
            var aulaOrigem = await mediator.Send(new ObterAulaPorIdQuery(request.AulaId));

            await AlterarRecorrencia(request, aulaOrigem, turma);

            return true;
        }

        private void ValidarQuantidadeAulasCJ(AlterarAulaRecorrenteCommand request)
        {
            if (request.Usuario.EhProfessorCj() && request.Quantidade > 2)
                throw new NegocioException("Quantidade de aulas por dia/disciplina excedido.");
        }

        private async Task ValidarComponentesProfessor(AlterarAulaRecorrenteCommand aulaRecorrente)
        {
            if (aulaRecorrente.Usuario.EhProfessorCj())
            {
                if (aulaRecorrente.Usuario.EhProfessorCj() && aulaRecorrente.Quantidade > 2)
                    throw new NegocioException("Quantidade de aulas por dia/disciplina excedido.");

                var componentesCurricularesDoProfessorCJ = await mediator.Send(new ObterComponentesCurricularesDoProfessorCJNaTurmaQuery(aulaRecorrente.Usuario.Login));
                if (componentesCurricularesDoProfessorCJ == null || !componentesCurricularesDoProfessorCJ.Any(c => c.TurmaId == aulaRecorrente.CodigoTurma && c.DisciplinaId == aulaRecorrente.ComponenteCurricularId))
                {
                    throw new NegocioException("Você não pode criar aulas para essa Turma.");
                }
            }
            else
            {
                var componentesCurricularesDoProfessor = await mediator.Send(new ObterComponentesCurricularesDoProfessorNaTurmaQuery(aulaRecorrente.CodigoTurma, aulaRecorrente.Usuario.Login, aulaRecorrente.Usuario.PerfilAtual));
                if (componentesCurricularesDoProfessor == null || !componentesCurricularesDoProfessor.Any(c => c.Codigo == aulaRecorrente.ComponenteCurricularId))
                {
                    throw new NegocioException("Você não pode criar aulas para essa Turma.");
                }

                var usuarioPodePersistirTurmaNaData = await mediator.Send(new ObterUsuarioPossuiPermissaoNaTurmaEDisciplinaQuery(aulaRecorrente.ComponenteCurricularId, aulaRecorrente.CodigoTurma, aulaRecorrente.DataAula, aulaRecorrente.Usuario));
                if (!usuarioPodePersistirTurmaNaData)
                    throw new NegocioException("Você não pode fazer alterações ou inclusões nesta turma, disciplina e data.");
            }
        }

        private async Task AlterarRecorrencia(AlterarAulaRecorrenteCommand request, Aula aulaOrigem, Turma turma)
        {
            var listaAlteracoes = new List<(bool sucesso, bool erro, DateTime data, string mensagem)>();
            var dataAula = request.DataAula;
            var aulaPaiIdOrigem = aulaOrigem.AulaPaiId ?? aulaOrigem.Id;

            var fimRecorrencia = await mediator.Send(new ObterFimPeriodoRecorrenciaQuery(request.TipoCalendarioId, aulaOrigem.DataAula.Date, request.RecorrenciaAula));
            var aulasDaRecorrencia = await repositorioAula.ObterAulasRecorrencia(aulaPaiIdOrigem, aulaOrigem.Id, fimRecorrencia);
            var listaProcessos = await IncluirAulasEmManutencao(aulaOrigem, aulasDaRecorrencia);

            try
            {
                listaAlteracoes.Add(await TratarAlteracaoAula(request, aulaOrigem, dataAula, turma));

                foreach(var aulaDaRecorrencia in aulasDaRecorrencia)
                {
                    dataAula = dataAula.AddDays(7);

                    listaAlteracoes.Add(await TratarAlteracaoAula(request, aulaDaRecorrencia, dataAula, turma));
                }
            }
            finally
            {
                await RemoverAulasEmManutencao(listaProcessos);
            }

            await NotificarUsuario(request.AulaId, listaAlteracoes, request.Usuario, request.NomeComponenteCurricular, turma);
        }

        private async Task RemoverAulasEmManutencao(IEnumerable<ProcessoExecutando> listaProcessos)
        {
            foreach(var processo in listaProcessos)
            {
                try
                {
                    await mediator.Send(new RemoverProcessoEmExecucaoCommand(processo));
                }
                catch (Exception ex)
                {
                    SentrySdk.AddBreadcrumb("Exclusao de Registro em Manutenção da Aula", "Alteração de Aula Recorrente");
                    SentrySdk.CaptureException(ex);
                }            
            }
        }

        private async Task<IEnumerable<ProcessoExecutando>> IncluirAulasEmManutencao(Aula aulaOrigem, IEnumerable<Aula> aulasDaRecorrencia)
        {
            var listaProcessos = new List<ProcessoExecutando>();

            listaProcessos.Add(await mediator.Send(new InserirAulaEmManutencaoCommand(aulaOrigem.Id)));
            foreach(var aulaRecorrente in aulasDaRecorrencia)
            {
                listaProcessos.Add(await mediator.Send(new InserirAulaEmManutencaoCommand(aulaRecorrente.Id)));
            }

            return listaProcessos;
        }

        private async Task<(bool sucesso, bool erro, DateTime data, string mensagem)> TratarAlteracaoAula(AlterarAulaRecorrenteCommand request, Aula aula, DateTime dataAula, Turma turma)
        {
            try
            {
                await AplicarValidacoes(request, dataAula, turma);

                await AlterarAula(request, aula, dataAula, turma);
            }
            catch (NegocioException ne)
            {
                // retorna erro = false
                return (false, false, dataAula, ne.Message);
            }
            catch (Exception e)
            {
                SentrySdk.AddBreadcrumb("Erro alterando aula recorrente", "Alteração de Aula Recorrente");
                SentrySdk.CaptureException(e);
                // retorna erro = true
                return (false, true, dataAula, e.Message);
            }

            // Retorna sucesso = true
            return (true, false, dataAula, string.Empty);
        }

        private async Task AlterarAula(AlterarAulaRecorrenteCommand request, Aula aula, DateTime dataAula, Turma turma)
        {
            aula.DataAula = dataAula;
            aula.Quantidade = request.Quantidade;
            aula.RecorrenciaAula = request.RecorrenciaAula;

            if (request.AulaId == aula.Id)
                aula.AulaPaiId = null;
            else 
                aula.AulaPaiId = request.AulaId;

            await repositorioAula.SalvarAsync(aula);
        }

        private async Task AplicarValidacoes(AlterarAulaRecorrenteCommand request, DateTime dataAula, Turma turma)
        {
            await ValidarSeEhDiaLetivo(request.TipoCalendarioId, dataAula, turma);

            var aulasExistentes = await mediator.Send(new ObterAulasPorDataTurmaComponenteCurricularEProfessorQuery(request.DataAula, request.CodigoTurma, request.ComponenteCurricularId, request.Usuario.CodigoRf));
            aulasExistentes = aulasExistentes.Where(a => a.Id != request.AulaId);
            if (aulasExistentes != null && aulasExistentes.Any(c => c.TipoAula == request.TipoAula))
                throw new NegocioException("Já existe uma aula criada neste dia para este componente curricular");

            await ValidarGrade(request, dataAula, aulasExistentes, turma);
        }

        private async Task ValidarSeEhDiaLetivo(long tipoCalendarioId, DateTime dataAula, Turma turma)
        {
            var consultaPodeCadastrarAula = await mediator.Send(new ObterPodeCadastrarAulaPorDataQuery()
            {
                UeCodigo = turma.Ue.CodigoUe,
                DreCodigo = turma.Ue.Dre.CodigoDre,
                TipoCalendarioId = tipoCalendarioId,
                DataAula = dataAula,
                Turma = turma
            });

            if (!consultaPodeCadastrarAula.PodeCadastrar)
                throw new NegocioException(consultaPodeCadastrarAula.MensagemPeriodo);
        }

        private async Task ValidarGrade(AlterarAulaRecorrenteCommand request, DateTime dataAula, IEnumerable<AulaConsultaDto> aulasExistentes, Turma turma)
        {
            var retornoValidacao = await mediator.Send(new ValidarGradeAulaCommand(turma.CodigoTurma,
                                                                                   turma.ModalidadeCodigo,
                                                                                   request.ComponenteCurricularId,
                                                                                   dataAula,
                                                                                   request.Usuario.CodigoRf,
                                                                                   request.Quantidade,
                                                                                   request.EhRegencia,
                                                                                   aulasExistentes));

            if (!retornoValidacao.resultado)
                throw new NegocioException(retornoValidacao.mensagem);
        }

        private async Task NotificarUsuario(long aulaId, IEnumerable<(bool sucesso, bool erro, DateTime data, string mensagem)> listaAlteracoes, Usuario usuario, string componenteCurricularNome, Turma turma)
        {
            var perfilAtual = usuario.PerfilAtual;
            if (perfilAtual == Guid.Empty)
                throw new NegocioException($"Não foi encontrado o perfil do usuário informado.");

            var tituloMensagem = $"Alteração de Aulas de {componenteCurricularNome} na turma {turma.Nome}";
            StringBuilder mensagemUsuario = new StringBuilder();

            var aulasAlteradas = listaAlteracoes.Where(c => c.sucesso);
            var aulasComConsistencia = listaAlteracoes.Where(c => !c.sucesso && !c.erro);
            var aulasComErro = listaAlteracoes.Where(c => !c.sucesso && c.erro);

            mensagemUsuario.Append($"Foram alteradas {aulasAlteradas.Count()} aulas da disciplina {componenteCurricularNome} para a turma {turma.Nome} da {turma.Ue?.Nome} ({turma.Ue?.Dre?.Nome}).");

            if (aulasComConsistencia.Any())
            {
                mensagemUsuario.Append($"<br><br>Não foi possível alterar aulas nas seguintes datas:<br>");
                foreach (var aula in aulasComConsistencia.OrderBy(a => a.data))
                {
                    mensagemUsuario.Append($"<br /> {aula.data:dd/MM/yyyy} - {aula.mensagem}");
                }
            }

            if (aulasComErro.Any())
            {
                mensagemUsuario.Append($"<br><br>Ocorreram erros na alteração das seguintes aulas:<br>");
                foreach (var aula in aulasComErro.OrderBy(a => a.data))
                {
                    mensagemUsuario.Append($"<br /> {aula.data:dd/MM/yyyy} - {aula.mensagem};");
                }
            }

            var notificacao = new Notificacao()
            {
                Ano = DateTime.Now.Year,
                Categoria = NotificacaoCategoria.Aviso,
                DreId = turma.Ue.Dre.CodigoDre,
                Mensagem = mensagemUsuario.ToString(),
                UsuarioId = usuario.Id,
                Tipo = NotificacaoTipo.Calendario,
                Titulo = tituloMensagem,
                TurmaId = turma.CodigoTurma,
                UeId = turma.Ue.CodigoUe,
            };

            unitOfWork.IniciarTransacao();
            try
            {
                // Salva Notificação
                servicoNotificacao.Salvar(notificacao);

                // Gera vinculo Notificacao x Aula
                await repositorioNotificacaoAula.Inserir(notificacao.Id, aulaId);

                unitOfWork.PersistirTransacao();
            }
            catch (Exception)
            {
                unitOfWork.Rollback();
                throw;
            }
        }

    }
}