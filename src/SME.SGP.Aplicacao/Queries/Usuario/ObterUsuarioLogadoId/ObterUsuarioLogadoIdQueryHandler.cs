﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ObterUsuarioLogadoIdQueryHandler : IRequestHandler<ObterUsuarioLogadoIdQuery, long>
    {
        private readonly IRepositorioUsuarioConsulta repositorioUsuario;
        private readonly IMediator mediator;

        public ObterUsuarioLogadoIdQueryHandler(IRepositorioUsuarioConsulta repositorioUsuario, IMediator mediator)
        {
            this.repositorioUsuario = repositorioUsuario ?? throw new ArgumentNullException(nameof(repositorioUsuario));
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<long> Handle(ObterUsuarioLogadoIdQuery request, CancellationToken cancellationToken)
        {
            var login = await mediator.Send(ObterLoginAtualQuery.Instance);
            if (string.IsNullOrWhiteSpace(login))
                throw new NegocioException("Usuário não encontrado.");

            var usuario = await repositorioUsuario.ObterPorCodigoRfLogin(string.Empty, login);

            if (usuario == null)
            {
                throw new NegocioException("Usuário não encontrado.");
            }

            return usuario.Id;

        }
    }
}
