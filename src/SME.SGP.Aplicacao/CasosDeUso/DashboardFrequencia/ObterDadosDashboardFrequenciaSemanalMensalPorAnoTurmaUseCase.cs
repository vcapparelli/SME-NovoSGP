﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Enumerados;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using SME.SGP.Infra.Dtos;

namespace SME.SGP.Aplicacao
{
    public class ObterDadosDashboardFrequenciaSemanalMensalPorAnoTurmaUseCase : AbstractUseCase, IObterDadosDashboardFrequenciaSemanalMensalPorAnoTurmaUseCase
    {
        public ObterDadosDashboardFrequenciaSemanalMensalPorAnoTurmaUseCase(IMediator mediator) : base(mediator)
        {
        }

        public async Task<IEnumerable<GraficoFrequenciaSemanalMensalDTO>> Executar(int anoLetivo, long dreId, long ueId, int modalidade, long[] turmaIds, DateTime? dataInicio, DateTime? datafim, int? mes, int tipoPeriodoDashboard, bool visaoDre = false)
        {
            var tipoConsolidadoFrequencia = (int)TipoConsolidadoFrequencia.Semanal;
                
            if (tipoPeriodoDashboard == (int)TipoConsolidadoFrequencia.Mensal)
            {
                tipoConsolidadoFrequencia = (int)TipoConsolidadoFrequencia.Mensal;

                dataInicio = new DateTime(DateTimeExtension.HorarioBrasilia().Year, mes.Value, 1);
                datafim = dataInicio.Value.AddMonths(1).AddDays(-1);
            }

            var frequenciaSemanalMensalDtos = await mediator.Send(new ObterFrequenciasConsolidadasPorTurmaMensalSemestralQuery(anoLetivo, dreId, ueId, modalidade, turmaIds, dataInicio.Value, datafim.Value, tipoConsolidadoFrequencia, visaoDre));

            if (frequenciaSemanalMensalDtos != null && frequenciaSemanalMensalDtos.Any())
            {
                return frequenciaSemanalMensalDtos.Select(s => new GraficoFrequenciaSemanalMensalDTO()
                {
                    Descricao = s.Descricao,
                    QuantidadeAbaixoMinimoFrequencia = s.QuantidadeAbaixoMinimoFrequencia,
                    QuantidadeAcimaMinimoFrequencia = s.QuantidadeAcimaMinimoFrequencia
                });
            }
            return Enumerable.Empty<GraficoFrequenciaSemanalMensalDTO>();
        }
    }
}