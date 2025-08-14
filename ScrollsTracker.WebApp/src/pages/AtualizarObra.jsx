import { useQuery, useMutation } from "@tanstack/react-query";
import { useNavigate, useParams } from "react-router-dom";
import { useState } from "react";
import ObraForm from "../components/ObraForm";
import Modal from "../components/Modal";

// Funções da API
const buscarObraPorIdApi = async (id) => {
  const response = await fetch(
    `https://localhost:7071/api/ScrollsTracker/ObterObra/${id}`
  );
  if (!response.ok) throw new Error("Obra não encontrada");
  var resposta = await response.json();
  return resposta;
};

const atualizarObraApi = async (data) => {
  const response = await fetch(
    `https://localhost:7071/api/ScrollsTracker/AtualizarObra`,
    {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data),
    }
  );
  if (!response.ok) throw new Error("Falha ao atualizar");
  return response.json();
};

function AtualizarObra() {
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

  var imageUrl = obraData?.imagem ? obraData.imagem : "";

  const deletarObraApi = async () => {
    const response = await fetch(
      `https://localhost:7071/api/ScrollsTracker/DeletarObra/${id}`,
      {
        method: "DELETE",
      }
    );
    if (!response.ok) throw new Error("Falha ao deletar");
    return response.json();
  };

  // 2. Cria a mutação para ATUALIZAR a obra
  const { mutate: atualizarObra, isPending: isUpdating } = useMutation({
    mutationFn: atualizarObraApi,
    onSuccess: () => {
      setIsOpenAtualizar(true);
    },
    onError: (error) => alert(error.message),
  });

  const { mutate: deletarObra, isPending: isDeleting } = useMutation({
    mutationFn: deletarObraApi,
    onSuccess: () => {
      navigate("/");
    },
    onError: (error) => alert(error.message),
  });

  const [isOpenAtualizar, setIsOpenAtualizar] = useState(false);
  const [isOpenDeletar, setIsOpenDeletar] = useState(false);

  if (isLoading)
    return <div className="text-white">Carregando dados da obra...</div>;
  if (isError)
    return <div className="text-red-500">Erro ao carregar os dados.</div>;

  return (
    <div
      className="min-h-screen flex flex-col items-center justify-center bg-no-repeat bg-cover "
      style={{
        backgroundImage: `url(${imageUrl})`,
      }}
    >
      <div
        className="min-h-full min-w-full flex flex-col items-center justify-center border border-white/20 
          shadow-lg
          bg-black/50 
          backdrop-blur-lg 
          text-white
          pt-4"
      >
        <h2 className="text-3xl font-bold text-white mb-6">Editar Obra</h2>
        <button onClick={() => setIsOpenDeletar(true)}>Abrir Modal</button>
        <Modal
          isOpen={isOpenAtualizar}
          onClose={() => setIsOpenAtualizar(false)}
        >
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
        <Modal isOpen={isOpenDeletar} onClose={() => setIsOpenDeletar(false)}>
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
          initialData={obraData}
          onSubmit={atualizarObra}
          isSubmitting={isUpdating}
          submitButtonText="Salvar Alterações"
          Type={"Atualizar"}
          onDelete={setIsOpenDeletar}
          isDeleting={isDeleting}
        />
      </div>
    </div>
  );
}

export default AtualizarObra;
