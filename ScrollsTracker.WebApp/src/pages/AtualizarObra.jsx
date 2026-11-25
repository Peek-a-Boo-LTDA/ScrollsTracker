import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useNavigate, useOutletContext, useParams } from "react-router-dom";
import { XCircleIcon, CheckCircleIcon } from "@heroicons/react/24/outline";
import { useState } from "react";
import ObraForm from "../components/ObraForm";
import HistoricoLancamentos from "../components/HistoricoLancamentos";
import Modal from "../components/Modal";

import { apiFetch } from "../api";

// Funções da API
const buscarObraPorIdApi = async (id) => {
  const response = await apiFetch(`/api/ScrollsTracker/ObterObra/${id}`);
  if (!response.ok) throw new Error("Obra não encontrada");
  var resposta = await response.json();
  return resposta;
};

function AtualizarObra() {
  const { podeGerenciarObras } = useOutletContext();
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const { id } = useParams();
  const {
    data: obraData,
    isLoading,
    isError,
  } = useQuery({
    queryKey: ["obra", id], // A chave da query inclui o ID
    queryFn: () => buscarObraPorIdApi(id),
  });

  const deletarObraApi = async () => {
    const response = await apiFetch(
      `/api/ScrollsTracker/DeletarObra/${id}`,
      {
        method: "DELETE",
      }
    );
    if (!response.ok) throw new Error("Falha ao deletar");
    return response.json();
  };

  // 2. Cria a mutação para ATUALIZAR a obra
  const { mutate: atualizarObra, isPending: isUpdating } = useMutation({
    mutationFn: async (dados) => {
      const rota = podeGerenciarObras ? `AtualizarObra/${id}` : `Obras/${id}/leitura`;
      const leitura = {
        ultimoCapituloLido: dados.ultimoCapituloLido,
        statusLeitura: dados.statusLeitura,
      };
      const resposta = await apiFetch(`/api/ScrollsTracker/${rota}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(podeGerenciarObras ? dados : leitura),
      });
      if (!resposta.ok) throw new Error("Falha ao salvar as alterações.");
    },
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["obra", id] }),
        queryClient.invalidateQueries({ queryKey: ["obras"] }),
        queryClient.invalidateQueries({ queryKey: ["lancamentos"] }),
      ]);
      setIsOpenAtualizar(true);
    },
    onError: (error) => {
      setIsOpenError(true);
      console.log(error);
    },
  });

  const { mutate: deletarObra, isPending: isDeleting } = useMutation({
    mutationFn: deletarObraApi,
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["obras"] }),
        queryClient.invalidateQueries({ queryKey: ["lancamentos"] }),
      ]);
      queryClient.removeQueries({ queryKey: ["obra", id] });
      navigate("/");
    },
    onError: (error) => {
      setIsOpenError(true);
      console.log(error);
    },
  });

  const [isOpenAtualizar, setIsOpenAtualizar] = useState(false);
  const [isOpenDeletar, setIsOpenDeletar] = useState(false);
  const [isOpenError, setIsOpenError] = useState(false);

  if (isLoading)
    return <div className="text-white">Carregando dados da obra...</div>;
  if (isError)
    return <div className="text-red-500">Erro ao carregar os dados.</div>;

  return (
    <div className="flex min-h-screen flex-col items-center px-2 py-8">
      <h1 className="page-title mb-2">
        {podeGerenciarObras ? "Editar obra" : "Minha leitura"}
      </h1>
      {!podeGerenciarObras && (
        <p className="px-4 text-center text-sm text-gray-300">
          Você pode editar seu status de leitura e o último capítulo lido.
        </p>
      )}
      <Modal
        isOpen={isOpenAtualizar}
        onClose={() => setIsOpenAtualizar(false)}
      >
        <CheckCircleIcon className="h-16 w-16 text-green-500 mb-4" />
        <p className="text-lg font-semibold text-gray-800 mb-4">
          Atualizado com sucesso
        </p>
        <button
          onClick={() => {
            setIsOpenAtualizar(false);
            navigate("/biblioteca");
          }}
          className="bg-blue-500 text-white font-bold py-2 px-6 rounded-md hover:bg-blue-600 transition-colors"
        >
          OK
        </button>
      </Modal>
      <Modal isOpen={isOpenError} onClose={() => setIsOpenError(false)}>
        <XCircleIcon className="h-16 w-16 text-red-500 mb-4" />

        <p className="text-xl font-bold text-gray-800 mb-4">
          Falha ao atualizar obra!
        </p>

        <p className="text-gray-600 mb-6">
          Ocorreu um erro ao processar sua solicitação. Por favor, tente
          novamente.
        </p>

        <button
          onClick={() => setIsOpenError(false)}
          className="bg-blue-500 text-white font-bold py-2 px-6 rounded-md hover:bg-blue-600 transition-colors"
        >
          OK
        </button>
      </Modal>
      <Modal isOpen={podeGerenciarObras && isOpenDeletar} onClose={() => setIsOpenDeletar(false)}>
        <p className="text-xl font-bold text-red-600 mb-4">
          Tem certeza que deseja deletar?
        </p>
        <p className="text-gray-700 mb-6">Essa ação não pode ser desfeita.</p>
        <div className="flex space-x-4">
          {/* Botão de confirmação de deleção */}
          <button
            onClick={() => {
              deletarObra(id);
              setIsOpenDeletar(false);
            }}
            className="bg-red-500 text-white font-bold py-2 px-6 rounded-md hover:bg-red-600 transition-colors"
          >
            Sim, deletar
          </button>

          {/* Botão de cancelamento */}
          <button
            onClick={() => setIsOpenDeletar(false)}
            className="bg-gray-300 text-gray-800 font-bold py-2 px-6 rounded-md hover:bg-gray-400 transition-colors"
          >
            Cancelar
          </button>
        </div>
      </Modal>
      <ObraForm
        key={id}
        initialData={obraData}
        somenteLeitura={!podeGerenciarObras}
        onSubmit={atualizarObra}
        isSubmitting={isUpdating}
        submitButtonText={podeGerenciarObras ? "Salvar alterações" : "Salvar progresso"}
        Type={"Atualizar"}
        onDelete={podeGerenciarObras ? setIsOpenDeletar : undefined}
        isDeleting={isDeleting}
      />
      <HistoricoLancamentos id={id} />
    </div>
  );
}

export default AtualizarObra;
