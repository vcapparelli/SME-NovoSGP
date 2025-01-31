﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using SME.SGP.Infra.Dtos;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class AlterarCartaIntencoesObservacaoCommandHandler : IRequestHandler<AlterarCartaIntencoesObservacaoCommand, AuditoriaDto>
    {
        private readonly IRepositorioCartaIntencoesObservacao repositorioCartaIntencoesObservacao;
        private readonly IRepositorioTurmaConsulta repositorioTurmaConsulta;
        private readonly IMediator mediator;

        public AlterarCartaIntencoesObservacaoCommandHandler(IRepositorioCartaIntencoesObservacao repositorioCartaIntencoesObservacao, IRepositorioTurmaConsulta repositorioTurmaConsulta, IMediator mediator)
        {
            this.repositorioCartaIntencoesObservacao = repositorioCartaIntencoesObservacao ?? throw new System.ArgumentNullException(nameof(repositorioCartaIntencoesObservacao));
            this.repositorioTurmaConsulta = repositorioTurmaConsulta ?? throw new System.ArgumentNullException(nameof(repositorioTurmaConsulta));
            this.mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        public async Task<AuditoriaDto> Handle(AlterarCartaIntencoesObservacaoCommand request, CancellationToken cancellationToken)
        {
            var cartaIntencoesObservacao = await repositorioCartaIntencoesObservacao.ObterPorIdAsync(request.ObservacaoId);
            var usuarioLogado = await mediator.Send(ObterUsuarioLogadoQuery.Instance);
            if (cartaIntencoesObservacao == null)
                throw new NegocioException("Observação da carta de intenção não encontrada.");

            var turma = await repositorioTurmaConsulta.ObterTurmaComUeEDrePorId(cartaIntencoesObservacao.TurmaId);

            cartaIntencoesObservacao.ValidarUsuarioAlteracao(request.UsuarioId);

            cartaIntencoesObservacao.Observacao = request.Observacao;            

            await repositorioCartaIntencoesObservacao.SalvarAsync(cartaIntencoesObservacao);

            if(request.Observacao.Length < 200)
            {
                // Excluir Notificação especifica da observação 
                await mediator.Send(new PublicarFilaSgpCommand(RotasRabbitSgp.RotaExcluirNotificacaoObservacaoCartaIntencoes,
                       new ExcluirNotificacaoCartaIntencoesObservacaoDto(cartaIntencoesObservacao.Id), Guid.NewGuid(), null));

                await mediator.Send(new PublicarFilaSgpCommand(RotasRabbitSgp.RotaNovaNotificacaoObservacaoCartaIntencoes,
                       new SalvarNotificacaoCartaIntencoesObservacaoDto(turma, usuarioLogado, cartaIntencoesObservacao.Id, request.Observacao), Guid.NewGuid(), null));
            }
            return (AuditoriaDto)cartaIntencoesObservacao;
        }

    }
}
