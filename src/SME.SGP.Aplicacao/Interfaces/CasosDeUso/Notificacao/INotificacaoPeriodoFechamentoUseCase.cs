﻿using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Text;

namespace SME.SGP.Aplicacao.Interfaces
{
    public interface INotificacaoPeriodoFechamentoUseCase : IUseCase<MensagemRabbit, bool>
    {
    }
}
