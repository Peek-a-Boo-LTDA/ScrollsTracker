import { XCircleIcon, CheckCircleIcon } from "@heroicons/react/24/outline";
import { useMutation } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import ObraForm from "../components/ObraForm";
import { useState } from "react";
import Modal from "../components/Modal";

function CadastrarObra() {
  const navigate = useNavigate();

  const cadastrarObraApi = async (data) => {
    console.log(JSON.stringify(data));
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
      setIsOpenCadastrar(true);
      // Poderia adicionar uma notificação de sucesso aqui (ex: react-toastify)
    },
    onError: (error) => {
      setIsOpenError(true);
      console.log(error);
    },
  });

  const [isOpenCadastrar, setIsOpenCadastrar] = useState(false);
  const [isOpenError, setIsOpenError] = useState(false);

  return (
    <div className="min-h-screen flex flex-col items-center justify-center p-4">
      <h2 className="text-3xl font-bold text-white mb-6">
        Cadastrar Nova Obra
      </h2>
      <ObraForm
        onSubmit={cadastrarObra}
        isSubmitting={isCadastrando}
        submitButtonText="Cadastrar Obra"
        Type="Cadastrar"
      />
      <Modal isOpen={isOpenCadastrar} onClose={() => setIsOpenCadastrar(false)}>
        <CheckCircleIcon className="h-16 w-16 text-green-500 mb-4" />
        <p className="text-lg font-semibold text-gray-800 mb-4">
          Cadastrado com sucesso!
        </p>
        <button
          onClick={() => {
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
    </div>
  );
}

export default CadastrarObra;
