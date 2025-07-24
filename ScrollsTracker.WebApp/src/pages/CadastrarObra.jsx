import { useMutation } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import ObraForm from "../components/ObraForm";

function CadastrarObra() {
  const navigate = useNavigate();

  const cadastrarObraApi = async (data) => {
    const response = await fetch(
      "https://localhost:7071/api/ScrollsTracker/CadastrarObra",
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data),
      }
    );
    if (!response.ok) {
      throw new Error("Falha ao cadastrar a obra.");
    }
    return response.json();
  };

  const { mutate: cadastrarObra, isPending: isCadastrando } = useMutation({
    mutationFn: cadastrarObraApi,
    onSuccess: (data) => {
      console.log("Resposta do servidor:", data);
      alert("Obra cadastrada com sucesso!");
      navigate("/biblioteca");
      // Poderia adicionar uma notificação de sucesso aqui (ex: react-toastify)
    },
    onError: (error) => {
      console.error("Erro ao cadastrar obra:", error);
      alert(error.message);
    },
  });

  return (
    <div className="min-h-screen bg-gray-900 flex flex-col items-center justify-center p-4">
      <h2 className="text-3xl font-bold text-white mb-6">
        Cadastrar Nova Obra
      </h2>
      <ObraForm
        onSubmit={cadastrarObra}
        isSubmitting={isCadastrando}
        submitButtonText="Cadastrar Obra"
        Type="Cadastrar"
      />
    </div>
  );
}

export default CadastrarObra;
