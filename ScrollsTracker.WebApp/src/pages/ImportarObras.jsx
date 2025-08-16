import React, { useState } from "react";

function ImportarObra() {
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

        const response = await fetch(
          "https://localhost:7071/api/ScrollsTracker/importar-obras",
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
        } else {
          setMessage("Erro ao importar os dados. Verifique o console.");
          const errorData = await response.json();
          console.error("Erro da API:", errorData);
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
      const response = await fetch(
        "https://localhost:7071/api/ScrollsTracker/exportar-obras"
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
    <div className="flex items-center justify-center text-white p-6 rounded-lg shadow-lg w-full h-screen">
      <div className="bg-gray-900 text-white p-6 rounded-lg shadow-lg w-2xl">
        <h3 className="text-2xl font-bold mb-4 border-b border-gray-700 pb-2">
          Importar Obras via JSON
        </h3>

        <div className="mb-4">
          <input
            type="file"
            accept=".json"
            onChange={handleFileChange}
            className="block w-full text-sm text-gray-300 file:mr-4 file:py-2 file:px-4
                 file:rounded-lg file:border-0
                 file:text-sm file:font-semibold
                 file:bg-blue-600 file:text-white
                 hover:file:bg-blue-500 cursor-pointer"
          />
        </div>

        <div className="flex gap-3">
          <button
            onClick={handleImport}
            disabled={!selectedFile}
            className={`flex-1 py-2 px-4 rounded-lg font-semibold transition-colors duration-200
                  ${
                    selectedFile
                      ? "bg-green-600 hover:bg-green-500"
                      : "bg-gray-700 cursor-not-allowed text-gray-400"
                  }`}
          >
            Importar
          </button>
          <button
            onClick={handleExport}
            className="flex-1 py-2 px-4 rounded-lg font-semibold bg-purple-600 hover:bg-purple-500 transition-colors duration-200"
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
