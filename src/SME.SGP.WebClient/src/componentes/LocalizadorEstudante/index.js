import PropTypes from 'prop-types';
import React, { useCallback, useEffect, useState } from 'react';
import { Label } from '~/componentes';
import { erros } from '~/servicos/alertas';
import InputCodigo from './componentes/InputCodigo';
import InputNome from './componentes/InputNome';
import service from './services/LocalizadorService';
import { store } from '~/redux';
import { setAlunosCodigo } from '~/redux/modulos/localizadorEstudante/actions';

const LocalizadorEstudante = props => {
  const { onChange, showLabel, desabilitado, dreId, ueId, anoLetivo } = props;

  const [dataSource, setDataSource] = useState([]);
  const [pessoaSelecionada, setPessoaSelecionada] = useState({});
  const [desabilitarCampo, setDesabilitarCampo] = useState({
    codigo: false,
    nome: false,
  });

  const onChangeNome = async valor => {
    if (valor.length === 0) {
      setPessoaSelecionada({
        alunoCodigo: '',
        alunoNome: '',
      });
      setTimeout(() => {
        setDesabilitarCampo(() => ({
          codigo: false,
          nome: false,
        }));
      }, 200);
    }

    if (valor.length < 3) return;

    const retorno = await service
      .buscarPorNome({
        nome: valor,
        codigoUe: ueId,
        anoLetivo,
      })
      .catch(e => erros(e));

    if (retorno && retorno?.data?.items?.length > 0) {
      setDataSource(
        retorno.data.items.map(aluno => ({
          alunoCodigo: aluno.codigo,
          alunoNome: aluno.nome,
        }))
      );
    }
  };

  const onBuscarPorCodigo = async codigo => {
    const retorno = await service
      .buscarPorCodigo({
        codigo: codigo.codigo,
        codigoUe: ueId,
        anoLetivo,
      })
      .catch(e => erros(e));

    if (retorno?.data?.items?.length > 0) {
      const { codigo, nome } = retorno.data.items[0];
      setDataSource(
        retorno.data.items.map(aluno => ({
          alunoCodigo: aluno.codigo,
          alunoNome: aluno.nome,
        }))
      );
      setPessoaSelecionada({
        alunoCodigo: parseInt(codigo, 10),
        alunoNome: nome,
      });
      setDesabilitarCampo(estado => ({
        ...estado,
        nome: true,
      }));
    }
  };

  const onChangeCodigo = valor => {
    if (valor.length === 0) {
      setPessoaSelecionada({
        alunoCodigo: '',
        alunoNome: '',
      });
      setDesabilitarCampo(estado => ({
        ...estado,
        nome: false,
      }));
    }
  };

  const onSelectPessoa = objeto => {
    setPessoaSelecionada({
      alunoCodigo: parseInt(objeto.key, 10),
      alunoNome: objeto.props.value,
    });
    setDesabilitarCampo(estado => ({
      ...estado,
      codigo: true,
    }));
  };

  useEffect(() => {
    const dados = [pessoaSelecionada.alunoCodigo];
    store.dispatch(setAlunosCodigo(dados));
  }, [pessoaSelecionada]);

  return (
    <>
      <div className="col-sm-12 col-md-6 col-lg-8 col-xl-8">
        {showLabel && <Label text="Nome" control="alunoNome" />}
        <InputNome
          dataSource={dataSource}
          onSelect={onSelectPessoa}
          onChange={onChangeNome}
          pessoaSelecionada={pessoaSelecionada}
          name="alunoNome"
          desabilitado={desabilitado || desabilitarCampo.nome}
        />
      </div>
      <div className="col-sm-12 col-md-6 col-lg-4 col-xl-4">
        {showLabel && <Label text="Código EOL" control="alunoCodigo" />}
        <InputCodigo
          pessoaSelecionada={pessoaSelecionada}
          onSelect={onBuscarPorCodigo}
          onChange={onChangeCodigo}
          name="alunoCodigo"
          desabilitado={desabilitado || desabilitarCampo.codigo}
        />
      </div>
    </>
  );
};

LocalizadorEstudante.propTypes = {
  onChange: () => {},
  showLabel: PropTypes.bool,
  desabilitado: PropTypes.bool,
  dreId: PropTypes.oneOfType([PropTypes.number, PropTypes.string]),
  ueId: PropTypes.oneOfType([PropTypes.number, PropTypes.string]),
  anoLetivo: PropTypes.oneOfType([PropTypes.number, PropTypes.string]),
};

LocalizadorEstudante.defaultProps = {
  onChange: PropTypes.func,
  showLabel: false,
  desabilitado: false,
  dreId: '',
  ueId: '',
  anoLetivo: '',
};

export default LocalizadorEstudante;