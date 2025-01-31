﻿using MediatR;
using Microsoft.Extensions.Configuration;
using SME.SGP.Aplicacao;
using SME.SGP.Aplicacao.Integracoes;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SME.SGP.Dominio.Servicos
{
    public class ServicoNotificacaoFrequencia : IServicoNotificacaoFrequencia
    {
        private readonly IConfiguration configuration;
        private readonly IRepositorioFrequenciaConsulta repositorioFrequencia;
        private readonly IRepositorioFrequenciaAlunoDisciplinaPeriodoConsulta repositorioFrequenciaAluno;
        private readonly IRepositorioNotificacaoFrequencia repositorioNotificacaoFrequencia;
        private readonly IRepositorioPeriodoEscolarConsulta repositorioPeriodoEscolar;
        private readonly IRepositorioComponenteCurricularConsulta repositorioComponenteCurricular;
        private readonly IRepositorioTurmaConsulta repositorioTurma;
        private readonly IRepositorioParametrosSistemaConsulta repositorioParametrosSistema;
        private readonly IRepositorioTipoCalendarioConsulta repositorioTipoCalendario;
        private readonly IServicoEol servicoEOL;
        private readonly IServicoNotificacao servicoNotificacao;
        private readonly IServicoUsuario servicoUsuario;
        private readonly IConsultasFeriadoCalendario consultasFeriadoCalendario;
        private readonly IMediator mediator;

        public ServicoNotificacaoFrequencia(IRepositorioNotificacaoFrequencia repositorioNotificacaoFrequencia,
                                            IRepositorioParametrosSistemaConsulta repositorioParametrosSistema,
                                            IRepositorioFrequenciaConsulta repositorioFrequencia,
                                            IRepositorioComponenteCurricularConsulta repositorioComponenteCurricular,
                                            IRepositorioTurmaConsulta repositorioTurma,
                                            IRepositorioPeriodoEscolarConsulta repositorioPeriodoEscolar,
                                            IRepositorioFrequenciaAlunoDisciplinaPeriodoConsulta repositorioFrequenciaAluno,
                                            IRepositorioTipoCalendarioConsulta repositorioTipoCalendario,
                                            IServicoNotificacao servicoNotificacao,
                                            IServicoUsuario servicoUsuario,
                                            IServicoEol servicoEOL,
                                            IConfiguration configuration,
                                            IMediator mediator, IConsultasFeriadoCalendario consultasFeriadoCalendario)
        {
            this.repositorioNotificacaoFrequencia = repositorioNotificacaoFrequencia ?? throw new ArgumentNullException(nameof(repositorioNotificacaoFrequencia));
            this.repositorioParametrosSistema = repositorioParametrosSistema ?? throw new ArgumentNullException(nameof(repositorioParametrosSistema));
            this.servicoNotificacao = servicoNotificacao ?? throw new ArgumentNullException(nameof(servicoNotificacao));
            this.repositorioFrequencia = repositorioFrequencia ?? throw new ArgumentNullException(nameof(repositorioFrequencia));
            this.repositorioFrequenciaAluno = repositorioFrequenciaAluno ?? throw new ArgumentNullException(nameof(repositorioFrequenciaAluno));
            this.servicoUsuario = servicoUsuario ?? throw new ArgumentNullException(nameof(servicoUsuario));
            this.repositorioTurma = repositorioTurma ?? throw new ArgumentNullException(nameof(repositorioTurma));
            this.repositorioPeriodoEscolar = repositorioPeriodoEscolar ?? throw new ArgumentNullException(nameof(repositorioPeriodoEscolar));
            this.repositorioTipoCalendario = repositorioTipoCalendario ?? throw new ArgumentNullException(nameof(repositorioTipoCalendario));
            this.servicoEOL = servicoEOL ?? throw new ArgumentNullException(nameof(servicoEOL));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.repositorioComponenteCurricular = repositorioComponenteCurricular ?? throw new ArgumentNullException(nameof(repositorioComponenteCurricular));
            this.consultasFeriadoCalendario = consultasFeriadoCalendario ?? throw new System.ArgumentNullException(nameof(consultasFeriadoCalendario));
            this.mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        #region Metodos Publicos

        public async Task ExecutaNotificacaoRegistroFrequencia()
        {
            var cargosNotificados = new List<(string CodigoTurma, Cargo? Cargo)>();

            Console.WriteLine($"Notificando usuários de aulas sem frequência.");

            cargosNotificados = await NotificarAusenciaFrequencia(TipoNotificacaoFrequencia.Professor, cargosNotificados);
            cargosNotificados = await NotificarAusenciaFrequencia(TipoNotificacaoFrequencia.SupervisorUe, cargosNotificados);
            await NotificarAusenciaFrequencia(TipoNotificacaoFrequencia.GestorUe, cargosNotificados);

            Console.WriteLine($"Rotina finalizada.");
        }

        public async Task NotificarAlunosFaltosos(long ueId)
        {
            var dataReferencia = DateTime.Today.AddDays(-1);

            var quantidadeDiasCP = int.Parse(await repositorioParametrosSistema.ObterValorPorTipoEAno(TipoParametroSistema.QuantidadeDiasNotificaoCPAlunosAusentes));
            var quantidadeDiasDiretor = int.Parse(await repositorioParametrosSistema.ObterValorPorTipoEAno(TipoParametroSistema.QuantidadeDiasNotificaoDiretorAlunosAusentes));
            

            //await NotificarAlunosFaltososModalidade(dataReferencia, ModalidadeTipoCalendario.Infantil, quantidadeDiasCP, quantidadeDiasDiretor);
            await NotificarAlunosFaltososModalidade(dataReferencia, ModalidadeTipoCalendario.FundamentalMedio, quantidadeDiasCP, quantidadeDiasDiretor, ueId);
            //await NotificarAlunosFaltososModalidade(dataReferencia, ModalidadeTipoCalendario.EJA, quantidadeDiasCP, quantidadeDiasDiretor);
        }

        private async Task NotificarAlunosFaltososModalidade(DateTime dataReferencia, ModalidadeTipoCalendario modalidade, int quantidadeDiasCP, int quantidadeDiasDiretor, long ueId)
        {
            var tipoCalendario = await repositorioTipoCalendario.BuscarPorAnoLetivoEModalidade(dataReferencia.Year, modalidade, dataReferencia.Semestre());

            await NotificaAlunosFaltososCargo(DiaRetroativo(dataReferencia, quantidadeDiasCP - 1), quantidadeDiasCP, Cargo.CP, tipoCalendario?.Id ?? 0, ueId);
            //await NotificaAlunosFaltososCargo(DiaRetroativo(dataReferencia, quantidadeDiasDiretor - 1), quantidadeDiasDiretor, Cargo.Diretor, tipoCalendario?.Id ?? 0);
        }

        public async Task VerificaRegraAlteracaoFrequencia(long registroFrequenciaId, DateTime criadoEm, DateTime alteradoEm)
        {
            int anoAtual = DateTime.Now.Year;

            // Parametro do sistema de dias para notificacao
            var qtdDiasParametroString = await repositorioParametrosSistema.ObterValorPorTipoEAno(
                                                    TipoParametroSistema.QuantidadeDiasNotificarAlteracaoChamadaEfetivada,
                                                   anoAtual);
            var parseado = int.TryParse(qtdDiasParametroString, out int qtdDiasParametro);

            if (!parseado)
                return;

            var qtdDiasAlteracao = (alteradoEm.Date - criadoEm.Date).TotalDays;

            // Verifica se ultrapassou o limite de dias para alteração
            if (qtdDiasAlteracao < qtdDiasParametro)
                return;

            var usuariosNotificacao = new List<(Cargo? Cargo, Usuario Usuario)>();

            // Dados da Aula
            var registroFrequencia = repositorioFrequencia.ObterAulaDaFrequencia(registroFrequenciaId);
            MeusDadosDto professor = await servicoEOL.ObterMeusDados(registroFrequencia.ProfessorRf);

            // Gestores
            var usuarios = BuscaGestoresUe(registroFrequencia.CodigoUe);
            if (usuarios != null)
                usuariosNotificacao.AddRange(usuarios);

            // Supervisores
            usuarios = BuscaSupervisoresUe(registroFrequencia.CodigoUe, usuariosNotificacao.Select(u => u.Cargo));
            if (usuarios != null)
                usuariosNotificacao.AddRange(usuarios);

            foreach (var usuario in usuariosNotificacao)
            {
                await NotificaAlteracaoFrequencia(usuario.Usuario, registroFrequencia, professor.Nome);
            }

        }

        public async Task NotificarAlunosFaltososBimestre()
        {
            // Notifica apenas no dia seguinte ao fim do bimestre
            var dataReferencia = DateTime.Today.AddDays(-1);
            var percentualCritico = double.Parse(await repositorioParametrosSistema.ObterValorPorTipoEAno(TipoParametroSistema.PercentualFrequenciaCritico, dataReferencia.Year));
            var percentualFrequenciaMinimaInfantil = double.Parse(await repositorioParametrosSistema.ObterValorPorTipoEAno(TipoParametroSistema.PercentualFrequenciaMinimaInfantil, dataReferencia.Year));

            await NotificaAlunosFaltososBimestreModalidade(dataReferencia, ModalidadeTipoCalendario.FundamentalMedio, percentualCritico);
            await NotificaAlunosFaltososBimestreModalidade(dataReferencia, ModalidadeTipoCalendario.EJA, percentualCritico, dataReferencia.Semestre());
            await NotificaAlunosFaltososBimestreModalidade(dataReferencia, ModalidadeTipoCalendario.Infantil, percentualFrequenciaMinimaInfantil);
        }
        #endregion Metodos Publicos

        #region Metodos Privados
        private async Task NotificaAlunosFaltososBimestreModalidade(DateTime dataReferencia, ModalidadeTipoCalendario modalidadeTipoCalendario, double percentualCritico, int semestre = 0)
        {
            var tipoCalendario = await repositorioTipoCalendario.BuscarPorAnoLetivoEModalidade(dataReferencia.Year, modalidadeTipoCalendario, semestre);
            var periodoEscolar = await repositorioPeriodoEscolar.ObterPorTipoCalendarioData(tipoCalendario?.Id ?? 0, dataReferencia);

            // Notifica apenas no dia seguinte ao fim do bimestre
            if (dataReferencia == periodoEscolar?.PeriodoFim)
            {
                var alunosFaltososBimestre = repositorioFrequenciaAluno.ObterAlunosFaltososBimestre(modalidadeTipoCalendario, percentualCritico, periodoEscolar.Bimestre, tipoCalendario?.AnoLetivo);

                foreach (var uesAgrupadas in alunosFaltososBimestre.GroupBy(a => new { a.DreCodigo, a.DreNome, a.DreAbreviacao, a.TipoEscola, a.UeCodigo, a.UeNome }))
                {
                    await NotificarEscolaAlunosFaltososBimestre(uesAgrupadas.Key.DreCodigo,
                                                                uesAgrupadas.Key.DreNome,
                                                                uesAgrupadas.Key.DreAbreviacao,
                                                                (TipoEscola)uesAgrupadas.Key.TipoEscola,
                                                                uesAgrupadas.Key.UeCodigo,
                                                                uesAgrupadas.Key.UeNome,
                                                                percentualCritico,
                                                                periodoEscolar.Bimestre,
                                                                dataReferencia.Year,
                                                                uesAgrupadas.GroupBy(u => u.TurmaCodigo),
                                                                modalidadeTipoCalendario);
                }
            }
        }

        private async Task NotificarEscolaAlunosFaltososBimestre(string dreCodigo, string dreNome, string dreAbreviacao, TipoEscola tipoEscola, string ueCodigo, string ueNome, double percentualCritico, int bimestre, int ano, IEnumerable<IGrouping<string, AlunoFaltosoBimestreDto>> turmasAgrupadas, ModalidadeTipoCalendario modalidadeTipoCalendario)
        {
            var titulo = $"Alunos com baixa frequência da {tipoEscola.ObterNomeCurto()} {ueNome} - {modalidadeTipoCalendario.Name()}";
            StringBuilder mensagem = new StringBuilder();
            mensagem.AppendLine($"<p>Abaixo segue a lista de turmas com alunos que tiveram frequência geral abaixo de <b>{percentualCritico}%</b> no <b>{bimestre}º bimestre</b> de <b>{ano}</b> da <b>{tipoEscola.ObterNomeCurto()} {ueNome} (DRE {dreAbreviacao})</b>.</p>");

            foreach (var turmaAgrupada in turmasAgrupadas)
            {
                var alunosDaTurma =  await  mediator.Send(new ObterAlunosEolPorTurmaQuery(turmaAgrupada.Key));
                var alunosFaltososTurma = alunosDaTurma.Where(c => turmaAgrupada.Any(a => a.AlunoCodigo == c.CodigoAluno));

                mensagem.AppendLine($"<p>Turma <b>{turmaAgrupada.First().TurmaModalidade.ObterNomeCurto()} - {turmaAgrupada.First().TurmaNome}</b></p>");
                mensagem.AppendLine("<table style='margin-left: auto; margin-right: auto;' border='2' cellpadding='5'>");
                mensagem.AppendLine("<tr>");
                mensagem.AppendLine("<td style='padding: 5px;'>Nº</td>");
                mensagem.AppendLine("<td style='padding: 5px;'>Nome do aluno</td>");
                mensagem.AppendLine("<td style='padding: 5px;'>Percentual de Frequência</td>");
                mensagem.AppendLine("</tr>");

                foreach (var aluno in alunosFaltososTurma.OrderBy(a => a.NomeAluno))
                {
                    var percentualFrequenciaAluno = 100 - turmaAgrupada.FirstOrDefault(c => c.AlunoCodigo == aluno.CodigoAluno).PercentualFaltas;

                    mensagem.AppendLine("<tr>");
                    mensagem.Append($"<td style='padding: 5px;'>{aluno.NumeroAlunoChamada}</td>");
                    mensagem.Append($"<td style='padding: 5px;'>{aluno.NomeAluno}</td>");
                    mensagem.Append($"<td style='text-align: center;'>{percentualFrequenciaAluno:0.00} %</td>");
                    mensagem.AppendLine("</tr>");
                }
            }

            var funcionariosEol = servicoNotificacao.ObterFuncionariosPorNivel(ueCodigo, Cargo.Supervisor);
            var functionariosEolCP = servicoNotificacao.ObterFuncionariosPorNivel(ueCodigo, Cargo.CP);
            var functionariosEolAD = servicoNotificacao.ObterFuncionariosPorNivel(ueCodigo, Cargo.AD);
            var functionariosEolDiretor = servicoNotificacao.ObterFuncionariosPorNivel(ueCodigo, Cargo.Diretor);

            await NotficarFuncionariosAlunosFaltososBimestre(funcionariosEol, titulo, mensagem.ToString(), ueCodigo, dreCodigo);
            await NotficarFuncionariosAlunosFaltososBimestre(functionariosEolCP, titulo, mensagem.ToString(), ueCodigo, dreCodigo);
            await NotficarFuncionariosAlunosFaltososBimestre(functionariosEolAD, titulo, mensagem.ToString(), ueCodigo, dreCodigo);
            await NotficarFuncionariosAlunosFaltososBimestre(functionariosEolDiretor, titulo, mensagem.ToString(), ueCodigo, dreCodigo);
        }

        private async Task NotficarFuncionariosAlunosFaltososBimestre(IEnumerable<(Cargo? Cargo, string Id)> funcionariosEol, string titulo, string mensagem, string ueCodigo, string dreCodigo)
        {
            foreach (var funcionario in funcionariosEol)
            {
                var usuario = servicoUsuario.ObterUsuarioPorCodigoRfLoginOuAdiciona(funcionario.Id);

                var notificacao = new Notificacao()
                {
                    Ano = DateTime.Now.Year,
                    Categoria = NotificacaoCategoria.Aviso,
                    Tipo = NotificacaoTipo.Frequencia,
                    Titulo = titulo,
                    Mensagem = mensagem,
                    UsuarioId = usuario.Id,
                    UeId = ueCodigo,
                    DreId = dreCodigo,
                };
                await servicoNotificacao.Salvar(notificacao);
            }
        }

        private async Task NotificaAlunosFaltososCargo(DateTime dataReferencia, int quantidadeDias, Cargo cargo, long tipoCalendarioId, long ueId)
        {
            var alunosFaltosos = await repositorioFrequencia.ObterAlunosFaltosos(dataReferencia, tipoCalendarioId, ueId);

            // Faltou em todas as aulas do dia e tem pelo menos 3 aulas registradas
            var alunosFaltasTodasAulasDoDia = ObterAlunosFaltososTodasAulas(alunosFaltosos);

            var alunosFaltasTodosOsDias = alunosFaltasTodasAulasDoDia
                .GroupBy(a => a.CodigoAluno)
                .Where(c => c.Count() >= quantidadeDias);

            // Agrupa por turma para notificação
            foreach (var turmaAgrupamento in alunosFaltasTodasAulasDoDia.GroupBy(a => a.TurmaCodigo))
            {
                // filtra alunos na turma que possuem faltas em todos os dias
                var alunosFaltososNaTurma = turmaAgrupamento.Where(c => alunosFaltasTodosOsDias.Any(a => a.Key == c.CodigoAluno));

                if (!alunosFaltososNaTurma.Any())
                    continue;

                var alunosTurmaEOL =  await  mediator.Send(new ObterAlunosEolPorTurmaQuery(turmaAgrupamento.Key));
                var turma = await repositorioTurma.ObterTurmaComUeEDrePorCodigo(turmaAgrupamento.Key);

                var alunosFaltososEOL = alunosTurmaEOL.Where(c => alunosFaltososNaTurma.Any(a => a.CodigoAluno == c.CodigoAluno));
                var funcionariosEol = await servicoNotificacao.ObterFuncionariosPorNivelAsync(turma.Ue.CodigoUe, cargo);

                if (funcionariosEol != null)
                {
                    foreach (var funcionarioEol in funcionariosEol)
                        await NotificacaoAlunosFaltososTurma(funcionarioEol.Id, alunosFaltososEOL, turma,
                            quantidadeDias);
                }
            }
        }

        private IEnumerable<AlunosFaltososDto> ObterAlunosFaltososTodasAulas(IEnumerable<AlunosFaltososDto> alunosFaltosos)
        {
            var anosFundamental = new string[] { "1", "2", "3", "4", "5" };
            return alunosFaltosos.Where(c => c.QuantidadeAulas == c.QuantidadeFaltas &&
                            ((c.ModalidadeCodigo == Modalidade.Fundamental && anosFundamental.Contains(c.Ano))
                            || c.QuantidadeAulas >= 3
                            || c.ModalidadeCodigo == Modalidade.EducacaoInfantil));
        }

        private async Task NotificacaoAlunosFaltososTurma(string funcionarioId, IEnumerable<AlunoPorTurmaResposta> alunos, Turma turma, int quantidadeDias)
        {
            var usuario = await servicoUsuario.ObterUsuarioPorCodigoRfLoginOuAdiciona(funcionarioId);

            var titulo = $"Alunos com excesso de ausências na turma {turma.Nome} ({turma.Ue.Nome})";
            StringBuilder mensagem = new StringBuilder();
            mensagem.AppendLine($"<p>O(s) seguinte(s) aluno(s) da turma <b>{turma.ModalidadeCodigo.ObterNomeCurto()}-{turma.Nome}</b> da <b>{turma.Ue.TipoEscola.ObterNomeCurto()} {turma.Ue.Nome} ({turma.Ue.Dre.Abreviacao})</b> está(ão) há {quantidadeDias} dias sem comparecer as aulas.</p>");

            mensagem.AppendLine("<table style='margin-left: auto; margin-right: auto;' border='2' cellpadding='5'>");
            mensagem.AppendLine("<tr>");
            mensagem.AppendLine("<td style='padding: 5px;'>Nº</td>");
            mensagem.AppendLine("<td style='padding: 5px;'>Nome do aluno</td>");
            mensagem.AppendLine("</tr>");

            foreach (var aluno in alunos.OrderBy(a => a.NomeAluno))
            {
                mensagem.AppendLine("<tr>");
                mensagem.Append($"<td style='padding: 5px;'>{aluno.NumeroAlunoChamada}</td>");
                mensagem.Append($"<td style='padding: 5px;'>{aluno.NomeAluno}</td>");
                mensagem.AppendLine("<tr>");
            }

            var notificacao = new Notificacao()
            {
                Ano = DateTime.Now.Year,
                Categoria = NotificacaoCategoria.Aviso,
                Tipo = NotificacaoTipo.Frequencia,
                Titulo = titulo,
                Mensagem = mensagem.ToString(),
                UsuarioId = usuario.Id,
                TurmaId = turma.CodigoTurma,
                UeId = turma.Ue.CodigoUe,
                DreId = turma.Ue.Dre.CodigoDre,
            };

            await servicoNotificacao.Salvar(notificacao);
        }

        /// <summary>
        /// Busca gestores da UE (CP > AD > Diretor)
        /// </summary>
        /// <param name="codigoUe">Código da UE</param>
        /// <returns>Lista de Cargos e Usuarios da gestão</returns>
        private IEnumerable<(Cargo? Cargo, Usuario Usuario)> BuscaGestoresUe(string codigoUe, Cargo cargo = Cargo.CP)
        {
            // Buscar gestor da Ue
            var funcionariosRetornoEol = servicoNotificacao.ObterFuncionariosPorNivel(codigoUe, cargo);

            if (funcionariosRetornoEol == null)
                return Enumerable.Empty<(Cargo? Cargo, Usuario Usuario)>();

            var usuarios = new List<(Cargo? Cargo, Usuario Usuario)>();
            foreach (var usuarioEol in funcionariosRetornoEol)
                usuarios.Add((usuarioEol.Cargo, servicoUsuario.ObterUsuarioPorCodigoRfLoginOuAdiciona(usuarioEol.Id).Result));

            var cargoNotificacao = funcionariosRetornoEol.GroupBy(f => f.Cargo).Select(f => f.Key).First();
            // Carrega só até o nível de Diretor
            if (!new[] { Cargo.Diretor, Cargo.Supervisor, Cargo.SupervisorTecnico }.Contains(cargoNotificacao.Value))
            {
                Cargo? proximoNivel = servicoNotificacao.ObterProximoNivel(cargoNotificacao, false);
                if (proximoNivel != null)
                {
                    var usuariosProximoNivel = BuscaGestoresUe(codigoUe, proximoNivel.Value);
                    if (usuariosProximoNivel != null && usuariosProximoNivel.Any())
                        usuarios.AddRange(usuariosProximoNivel);
                }
            }

            return usuarios;
        }

        private async Task<IEnumerable<(Cargo? Cargo, Usuario Usuario)>> BuscaProfessorAula(RegistroFrequenciaFaltanteDto turma)
        {
            if (turma.ModalidadeTurma == Modalidade.EducacaoInfantil)
            {
                var disciplinaEols = await mediator.Send(new ObterProfessoresTitularesDisciplinasEolQuery(turma.CodigoTurma));
                if (disciplinaEols != null)
                    foreach (var disciplina in disciplinaEols)
                    {
                        return await RetornaUsuarios(disciplina.ProfessorRf);
                    }
            }
            else
            {
                // Buscar professor da ultima aula
                var professorRf = turma.Aulas
                        .OrderBy(o => o.DataAula)
                        .Last().ProfessorId;

                return await this.RetornaUsuarios(professorRf);

            }

            return null;
        }

        private async Task<IEnumerable<(Cargo? Cargo, Usuario Usuario)>> RetornaUsuarios(string procurarRfs)
        {
            var rfs = procurarRfs.Split(new char[] { ',' });
            var usuarios = new List<(Cargo? Cargo, Usuario Usuario)>();

            foreach (var rf in rfs)
            {
                var usuario = await servicoUsuario.ObterUsuarioPorCodigoRfLoginOuAdiciona(rf.Trim());
                if (usuario != null)
                    usuarios.Add((null, usuario));
            }

            return usuarios;
        }

        private IEnumerable<(Cargo? Cargo, Usuario Usuario)> BuscaSupervisoresUe(string codigoUe, IEnumerable<Cargo?> cargosNotificados)
        {
            var funcionariosRetorno = servicoNotificacao.ObterFuncionariosPorNivel(codigoUe, Cargo.Supervisor);

            if (funcionariosRetorno == null || cargosNotificados.Any(c => funcionariosRetorno.Any(f => f.Cargo == c)))
                return Enumerable.Empty<(Cargo? Cargo, Usuario Usuario)>();

            var usuarios = new List<(Cargo? Cargo, Usuario Usuario)>();
            foreach (var funcionario in funcionariosRetorno)
                usuarios.Add((funcionario.Cargo, servicoUsuario.ObterUsuarioPorCodigoRfLoginOuAdiciona(funcionario.Id).Result));

            return usuarios;
        }

        private async Task<IEnumerable<(Cargo? Cargo, Usuario Usuario)>> BuscaUsuarioNotificacao(RegistroFrequenciaFaltanteDto turma, TipoNotificacaoFrequencia tipo)
        {
            IEnumerable<(Cargo? Cargo, Usuario Usuario)> usuarios = Enumerable.Empty<(Cargo?, Usuario)>();
            switch (tipo)
            {
                case TipoNotificacaoFrequencia.Professor:
                    usuarios = await BuscaProfessorAula(turma);
                    break;

                case TipoNotificacaoFrequencia.GestorUe:
                    usuarios = BuscaGestoresUe(turma.CodigoUe);
                    break;

                case TipoNotificacaoFrequencia.SupervisorUe:
                    usuarios = BuscaSupervisoresUe(turma.CodigoUe, usuarios.Select(u => u.Cargo));
                    break;

                default:
                    usuarios = await BuscaProfessorAula(turma);
                    break;
            }
            return usuarios;
        }

        private async Task NotificaAlteracaoFrequencia(Usuario usuario, RegistroFrequenciaAulaDto registroFrequencia, string usuarioAlteracao)
        {
            // carregar nomes da turma, escola, disciplina e professor para notificacao
            var disciplina = ObterNomeDisciplina(registroFrequencia.CodigoDisciplina);

            var tituloMensagem = $"Título: Alteração extemporânea de frequência  da turma {registroFrequencia.NomeTurma} no componente curricular {disciplina}.";

            StringBuilder mensagemUsuario = new StringBuilder();
            mensagemUsuario.Append($"O Professor {usuarioAlteracao} realizou alterações no registro de frequência do dia {registroFrequencia.DataAula.ToString("dd/MM/yyyy")}");
            mensagemUsuario.Append($" da turma {registroFrequencia.NomeTurma} da {registroFrequencia.NomeTipoEscola} {registroFrequencia.NomeUe} ({registroFrequencia.NomeDre}) no componente curricular {disciplina}.");
            mensagemUsuario.Append($"Para visualizar esse registro acesse 'Diário de Classe > Frequência/Plano Aula'");

            var notificacao = new Notificacao()
            {
                Ano = DateTime.Now.Year,
                Categoria = NotificacaoCategoria.Alerta,
                Tipo = NotificacaoTipo.Frequencia,
                Titulo = tituloMensagem,
                Mensagem = mensagemUsuario.ToString(),
                UsuarioId = usuario.Id,
                TurmaId = registroFrequencia.CodigoTurma,
                UeId = registroFrequencia.CodigoUe,
                DreId = registroFrequencia.CodigoDre,
            };

            await servicoNotificacao.Salvar(notificacao);
        }

        private async Task<List<(string CodigoTurma, Cargo? Cargo)>> NotificarAusenciaFrequencia(TipoNotificacaoFrequencia tipo, List<(string CodigoTurma, Cargo? Cargo)> cargosNotificados)
        {
            // Busca registro de aula sem frequencia e sem notificação do tipo
            IEnumerable<RegistroFrequenciaFaltanteDto> turmasSemRegistro = null;
            turmasSemRegistro = await mediator.Send(new ObterNotificacaoFrequenciaTurmasSemRegistroDeFrequenciaQuery(tipo));

            if (turmasSemRegistro != null)
            {
                // Busca parametro do sistema de quantidade de aulas sem frequencia para notificação
                var qtdAulasNotificacao = QuantidadeAulasParaNotificacao(tipo).Result;

                if (qtdAulasNotificacao.HasValue)
                {
                    foreach (var turma in turmasSemRegistro)
                    {
                        // Carrega todas as aulas sem registro de frequencia da turma e disciplina para notificação
                        turma.Aulas = repositorioFrequencia.ObterAulasSemRegistroFrequencia(turma.CodigoTurma, turma.DisciplinaId, tipo);
                        if (turma.Aulas != null && turma.Aulas.Count() >= qtdAulasNotificacao)
                        {
                            // Busca Professor/Gestor/Supervisor da Turma ou Ue
                            var usuarios = await BuscaUsuarioNotificacao(turma, tipo);

                            if (usuarios != null)
                            {
                                var cargosLinq = cargosNotificados;
                                var cargosNaoNotificados = usuarios.Select(u => u.Cargo)
                                                            .GroupBy(u => u)
                                                            .Where(w => !cargosLinq.Any(l => l.CodigoTurma == turma.CodigoTurma && l.Cargo == w.Key))
                                                            .Select(s => new { turma.CodigoTurma, s.Key });

                                foreach (var usuario in usuarios.Where(u => cargosNaoNotificados.Select(c => c.Key).Contains(u.Cargo)))
                                {
                                    NotificaRegistroFrequencia(usuario.Usuario, turma, tipo);
                                }

                                cargosNotificados.AddRange(cargosNaoNotificados.Select(n => (n.CodigoTurma, n.Key)));
                            }
                        }
                        else
                            Console.WriteLine($"Notificação não necessária pois quantidade de aulas sem frequência: {turma.Aulas?.Count() ?? 0 } está dentro do limite: {qtdAulasNotificacao}.");
                    }
                }
            }

            return cargosNotificados;
        }

        private async Task NotificaRegistroFrequencia(Usuario usuario, RegistroFrequenciaFaltanteDto turmaSemRegistro, TipoNotificacaoFrequencia tipo)
        {
            var disciplinas = await repositorioComponenteCurricular.ObterDisciplinasPorIds(new long[] { long.Parse(turmaSemRegistro.DisciplinaId) });
            if (disciplinas != null && disciplinas.Any() && disciplinas.FirstOrDefault().RegistraFrequencia)
            {
                var disciplina = disciplinas.FirstOrDefault();

                if (disciplina.RegistraFrequencia)
                {
                    var tituloMensagem = $"Frequência da turma {turmaSemRegistro.NomeTurma} - {turmaSemRegistro.DisciplinaId} ({turmaSemRegistro.NomeUe})";
                    StringBuilder mensagemUsuario = new StringBuilder();
                    mensagemUsuario.Append($"A turma a seguir esta a <b>{turmaSemRegistro.Aulas.Count()} aulas</b> sem registro de frequência da turma");
                    mensagemUsuario.Append("<br />");
                    mensagemUsuario.Append($"<br />Escola: <b>{turmaSemRegistro.NomeUe}</b>");
                    mensagemUsuario.Append($"<br />Turma: <b>{turmaSemRegistro.NomeTurma}</b>");
                    mensagemUsuario.Append($"<br />Componente Curricular: <b>{disciplina.Nome}</b>");
                    mensagemUsuario.Append($"<br />Aulas:");

                    mensagemUsuario.Append("<ul>");
                    foreach (var aula in turmaSemRegistro.Aulas)
                    {
                        mensagemUsuario.Append($"<li>Data: {aula.DataAula.ToString("dd/MM/yyyy")}</li>");
                    }
                    mensagemUsuario.Append("</ul>");

                    var hostAplicacao = configuration["UrlFrontEnd"];
                    var parametros = $"turma={turmaSemRegistro.CodigoTurma}&DataAula={turmaSemRegistro.Aulas.FirstOrDefault().DataAula.ToShortDateString()}&disciplina={turmaSemRegistro.DisciplinaId}";
                    mensagemUsuario.Append($"<a href='{hostAplicacao}diario-classe/frequencia-plano-aula?{parametros}'>Clique aqui para regularizar.</a>");

                    var notificacao = new Notificacao()
                    {
                        Ano = DateTime.Now.Year,
                        Categoria = NotificacaoCategoria.Alerta,
                        Tipo = NotificacaoTipo.Frequencia,
                        Titulo = tituloMensagem,
                        Mensagem = mensagemUsuario.ToString(),
                        UsuarioId = usuario.Id,
                        TurmaId = turmaSemRegistro.CodigoTurma,
                        UeId = turmaSemRegistro.CodigoUe,
                        DreId = turmaSemRegistro.CodigoDre,
                    };

                    await servicoNotificacao.Salvar(notificacao);

                    foreach (var aula in turmaSemRegistro.Aulas)
                    {
                        repositorioNotificacaoFrequencia.Salvar(new NotificacaoFrequencia()
                        {
                            Tipo = tipo,
                            NotificacaoCodigo = notificacao.Codigo,
                            AulaId = aula.Id,
                            DisciplinaCodigo = turmaSemRegistro.DisciplinaId
                        });
                    }
                }
            }
            else
                await mediator.Send(new SalvarLogViaRabbitCommand("Não foi possível obter o componente curricular pois o EOL não respondeu", Enumerados.LogNivel.Critico, Enumerados.LogContexto.Frequencia));
        }

        private async Task<string> ObterNomeDisciplina(string codigoDisciplina)
        {
            long[] disciplinaId = { long.Parse(codigoDisciplina) };
            var disciplina = await repositorioComponenteCurricular.ObterDisciplinasPorIds(disciplinaId);

            if (!disciplina.Any())
                throw new NegocioException("Componente curricular não encontrado no EOL.");

            return disciplina.FirstOrDefault().Nome;
        }

        private async Task<int?> QuantidadeAulasParaNotificacao(TipoNotificacaoFrequencia tipo)
        {
            TipoParametroSistema tipoParametroSistema;

            if (tipo == TipoNotificacaoFrequencia.Professor)
                tipoParametroSistema = TipoParametroSistema.QuantidadeAulasNotificarProfessor;
            else if (tipo == TipoNotificacaoFrequencia.GestorUe)
                tipoParametroSistema = TipoParametroSistema.QuantidadeAulasNotificarGestorUE;
            else
                tipoParametroSistema = TipoParametroSistema.QuantidadeAulasNotificarSupervisorUE;

            var qtdDias = await repositorioParametrosSistema.ObterValorPorTipoEAno(tipoParametroSistema, DateTime.Now.Year);

            return !string.IsNullOrEmpty(qtdDias) ? int.Parse(qtdDias) : (int?)null;
        }

        private DateTime DiaRetroativo(DateTime data, int nrDias)
        {

            int contadorDias = nrDias;
            DateTime dataRetorno = data;

            while (contadorDias > 0)
            {
                if (!dataRetorno.FimDeSemana() && !Feriado(dataRetorno))
                    contadorDias--;

                dataRetorno = dataRetorno.AddDays(-1);
            }

            return dataRetorno;
        }
        private bool Feriado(DateTime data)
        {
            FiltroFeriadoCalendarioDto filtro = new FiltroFeriadoCalendarioDto();
            filtro.Ano = data.Year;
            var ret = consultasFeriadoCalendario.Listar(filtro).Result;
            return ret.Any(x => x.DataFeriado == data);
        }

        #endregion Metodos Privados
    }
}