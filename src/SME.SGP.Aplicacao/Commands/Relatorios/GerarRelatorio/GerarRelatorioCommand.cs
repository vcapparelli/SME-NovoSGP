﻿using MediatR;
using SME.SGP.Dominio;

namespace SME.SGP.Aplicacao
{
    public class GerarRelatorioCommand : IRequest<bool>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tipoRelatorio">Endpoint do relatório no servidor de relatórios, descrito na tag DisplayName</param>
        /// <param name="filtros">Classe de filtro vindo do front</param>
        public GerarRelatorioCommand(TipoRelatorio tipoRelatorio, object filtros, long idUsuarioLogado)
        {
            TipoRelatorio = tipoRelatorio;
            Filtros = filtros;
            IdUsuarioLogado = idUsuarioLogado;
        }

        /// <summary>
        /// Classe de filtro vindo do front
        /// </summary>
        public object Filtros { get; set; }
        public TipoRelatorio TipoRelatorio { get; set; }
        public long IdUsuarioLogado { get; set; }
    }
}