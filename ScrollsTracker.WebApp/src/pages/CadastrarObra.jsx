import { XCircleIcon, CheckCircleIcon } from "@heroicons/react/24/outline";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { Navigate, useNavigate, useOutletContext } from "react-router-dom";
import ObraForm from "../components/ObraForm";
import { useState } from "react";
import Modal from "../components/Modal";

import { apiFetch } from "../api";

function CadastrarObra() {
  const { podeCadastrarObras } = useOutletContext();
  const queryClient = useQueryClient();
  const navigate = useNavigate();

  const cadastrarObraApi = async (data) => {
    const response = await apiFetch(
      `/api/ScrollsTracker/CadastrarObra`,
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
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["obras"] }),
        queryClient.invalidateQueries({ queryKey: ["lancamentos"] }),
      ]);
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

  if (!podeCadastrarObras) return <Navigate to="/biblioteca" replace />;

  return (
    <div className="flex min-h-screen flex-col items-center px-2 py-8">
      <h2 className="page-title mb-2">
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
