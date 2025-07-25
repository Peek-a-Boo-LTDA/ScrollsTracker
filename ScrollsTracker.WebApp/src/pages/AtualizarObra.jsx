import { useQuery, useMutation } from "@tanstack/react-query";
import { useNavigate, useParams } from "react-router-dom";
import ObraForm from "../components/ObraForm";

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
      alert("Obra atualizada com sucesso!");
      navigate("/biblioteca");
    },
    onError: (error) => alert(error.message),
  });

  const { mutate: deletarObra, isPending: isDeleting } = useMutation({
    mutationFn: deletarObraApi,
    onSuccess: () => {
      alert("Obra deletada com sucesso!");
      navigate("/");
    },
    onError: (error) => alert(error.message),
  });

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
        <ObraForm
          initialData={obraData}
          onSubmit={atualizarObra}
          isSubmitting={isUpdating}
          submitButtonText="Salvar Alterações"
          Type={"Atualizar"}
          onDelete={deletarObra}
          isDeleting={isDeleting}
        />
      </div>
    </div>
  );
}

export default AtualizarObra;
