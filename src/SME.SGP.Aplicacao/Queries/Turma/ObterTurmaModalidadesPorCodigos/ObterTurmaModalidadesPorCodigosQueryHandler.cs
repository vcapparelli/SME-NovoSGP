﻿using MediatR;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace SME.SGP.Aplicacao
{
    public class ObterTurmaModalidadesPorCodigosQueryHandler : IRequestHandler<ObterTurmaModalidadesPorCodigosQuery, IEnumerable<TurmaModalidadeCodigoDto>>
    {
        private readonly IRepositorioTurma repositorioTurma;

        public ObterTurmaModalidadesPorCodigosQueryHandler(IRepositorioTurma repositorioTurma)
        {
            this.repositorioTurma = repositorioTurma ?? throw new ArgumentNullException(nameof(repositorioTurma));
        }
        public async Task<IEnumerable<TurmaModalidadeCodigoDto>> Handle(ObterTurmaModalidadesPorCodigosQuery request, CancellationToken cancellationToken)
        {
            return await repositorioTurma.ObterModalidadePorCodigos(request.TurmasCodigo);            
        }
    }
}