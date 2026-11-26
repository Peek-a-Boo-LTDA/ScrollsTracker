import { useState } from "react";

import { useOutletContext } from "react-router-dom";
import { useQueryClient } from "@tanstack/react-query";
import { apiFetch } from "../api";

function ImportarObra() {
  const { podeCadastrarObras } = useOutletContext();
  const queryClient = useQueryClient();
  const [selectedFile, setSelectedFile] = useState(null);
  const [message, setMessage] = useState("");

  const handleFileChange = (event) => {
    const file = event.target.files[0];
    if (file && file.type === "application/json") {
      setSelectedFile(file);
      setMessage(`Arquivo selecionado: ${file.name}`);
    } else {
      setSelectedFile(null);
      setMessage("Por favor, selecione um arquivo JSON válido.");
    }
  };

  const handleImport = async () => {
    if (!selectedFile) {
      setMessage("Nenhum arquivo selecionado para importação.");
      return;
    }

    setMessage("Importando dados...");

    const reader = new FileReader();
    reader.onload = async (e) => {
      try {
        const jsonContent = e.target.result;
        const data = JSON.parse(jsonContent);
        console.log("Dados importados:", data);

        const response = await apiFetch(
          `/api/ScrollsTracker/importar-obras`,
          {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
            },
            body: JSON.stringify(data),
          }
        );

        if (response.ok) {
          setMessage("Dados importados com sucesso!");
          await Promise.all(["obras", "lancamentos", "obra", "continuar-lendo"].map((chave) =>
            queryClient.invalidateQueries({ queryKey: [chave] })
          ));
        } else {
          setMessage("Erro ao importar os dados. Verifique o console.");

        }
      } catch (error) {
        console.error("Erro ao processar o arquivo JSON:", error);
        setMessage("Erro ao processar o arquivo. Verifique o formato do JSON.");
      }
    };

    reader.readAsText(selectedFile);
  };

  const handleExport = async () => {
    try {
      // Busca os dados para exportar da API
      const response = await apiFetch(
        `/api/ScrollsTracker/exportar-obras`
      );
      if (!response.ok) throw new Error("Erro ao buscar dados");

      const data = await response.json();

      // Cria o arquivo JSON e força o download
      const blob = new Blob([JSON.stringify(data, null, 2)], {
        type: "application/json",
      });
      const url = URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = "dados-exportados.json";
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(url);

      setMessage("Dados exportados com sucesso!");
    } catch (error) {
      console.error("Erro ao exportar os dados:", error);
      setMessage("Erro ao exportar os dados.");
    }
  };

  return (
    <div className="page-width page-content flex justify-center">
      <div className="surface w-full max-w-xl p-6 sm:p-8">
        <h3 className="text-2xl font-bold mb-4 border-b border-gray-700 pb-2">
          Biblioteca em JSON
        </h3>

        {podeCadastrarObras && <div className="mb-4">
          <input
            type="file"
            accept=".json"
            onChange={handleFileChange}
            className="block w-full text-sm text-gray-300 file:mr-4 file:py-2 file:px-4
                 file:rounded-lg file:border-0
                 file:text-sm file:font-semibold
                 file:bg-neutral-200 file:text-neutral-950
                 hover:file:bg-neutral-300 cursor-pointer"
          />
        </div>}

        <div className="flex flex-wrap gap-3">
          {podeCadastrarObras && <button
            onClick={handleImport}
            disabled={!selectedFile}
            className="button button-primary flex-1"
          >
            Importar
          </button>}
          <button
            onClick={handleExport}
            className="button button-secondary flex-1"
          >
            Exportar
          </button>
        </div>

        <p className="mt-4 text-sm text-gray-400">{message}</p>
      </div>
    </div>
  );
}

export default ImportarObra;
