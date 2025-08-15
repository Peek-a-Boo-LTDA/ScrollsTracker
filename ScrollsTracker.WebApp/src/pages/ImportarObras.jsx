import React, { useState } from "react";

function ImportarObra() {
  const [selectedFile, setSelectedFile] = useState(null);
  const [message, setMessage] = useState("");

  const handleFileChange = (event) => {
    // Pega o primeiro arquivo selecionado
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

        // Envia o conteúdo JSON para a API
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
          setMessage(
            "Erro ao importar os dados. Verifique o console para mais detalhes."
          );
          const errorData = await response.json();
          console.error("Erro da API:", errorData);
        }
      } catch (error) {
        console.error("Erro ao processar o arquivo JSON:", error);
        setMessage(
          "Ocorreu um erro ao processar o arquivo. Verifique o formato do JSON."
        );
      }
    };

    // Lê o arquivo como texto
    reader.readAsText(selectedFile);
  };
  return (
    <div className="text-white">
      <h3>Importar Dados via JSON</h3>
      <input type="file" accept=".json" onChange={handleFileChange} />
      <button onClick={handleImport} disabled={!selectedFile}>
        Importar
      </button>
      <p>{message}</p>
    </div>
  );
}

export default ImportarObra;
