﻿using SME.SGP.Infra;
using SME.SGP.Infra.Dtos;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao.Interfaces
{
    public interface IExcluirNotificacaoCartaIntencoesObservacaoUseCase
    {
        Task<bool> Executar(MensagemRabbit mensagemRabbit);
    }
}