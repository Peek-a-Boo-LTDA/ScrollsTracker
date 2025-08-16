import React from "react";
import { Link } from "react-router-dom";

// Um pequeno componente helper para os badges, para deixar o código mais limpo.
// As cores são definidas com base no texto do status.
const StatusBadge = ({ text }) => {
  const colorClasses = {
    // StatusLeitura
    Lendo: "bg-blue-500 text-blue-100",
    Completo: "bg-green-500 text-green-100",
    Pausado: "bg-yellow-500 text-yellow-100",
    "Planejo Ler": "bg-gray-500 text-gray-100",
    Abandonado: "bg-red-500 text-red-100",
    // Status (da obra)
    "Em Andamento": "bg-teal-500 text-teal-100",
    Finalizado: "bg-purple-500 text-purple-100",
  };

  return (
    <span
      className={`inline-block px-2 py-1 text-xs font-semibold rounded-full ${
        colorClasses[text] || "bg-gray-600 text-gray-100"
      }`}
    >
      {text}
    </span>
  );
};

function MangaItem(props) {
  // Desestruturando todas as props necessárias
  const {
    id,
    titulo,
    descricao,
    imagem,
    totalCapitulos,
    ultimoCapituloLido,
    status,
    statusLeitura,
    dataAtualizacao,
  } = props;

  // 1. Lógica para a Barra de Progresso
  // Evita divisão por zero e garante que o progresso não passe de 100%
  const progresso =
    totalCapitulos > 0
      ? Math.min((ultimoCapituloLido / totalCapitulos) * 100, 100)
      : 0;

  // 2. Formatação da Data
  // Converte a string de data para um objeto Date e formata para o padrão local (ex: dd/mm/aaaa)
  const dataFormatada = new Date(dataAtualizacao).toLocaleDateString();

  return (
    // O Link agora envolve todo o card para torná-lo clicável
    <Link
      to={`/atualizar-obra/${id}`}
      className="block transform hover:scale-[1.02] transition-transform duration-200"
    >
      <div className="flex gap-4 p-4 m-3 bg-stone-900 rounded-lg shadow-md border border-gray-700 h-full">
        {/* Coluna da Imagem */}
        <div className="w-24 md:w-32 flex-shrink-0">
          <img
            src={imagem}
            alt={`Capa de ${titulo}`}
            className="w-full h-full object-cover rounded-md"
          />
        </div>

        {/* Coluna de Informações */}
        <div className="flex flex-col flex-grow justify-between max-w-[70%]">
          {/* Seção Superior: Título e Badges */}
          <div>
            <h2 className="text-lg font-bold text-white mb-2 line-clamp-2">
              {titulo}
            </h2>
            {/* 3. Renderização dos Badges */}
            <div className="flex flex-wrap gap-2 mb-3">
              {status && <StatusBadge text={status} />}
              {statusLeitura && <StatusBadge text={statusLeitura} />}
            </div>
          </div>
          <p className="text-white text-sm line-clamp-3 mb-1">{descricao}</p>
          {/* Seção Inferior: Progresso e Data */}
          <div>
            {/* Barra de Progresso */}
            <div className="mb-1">
              <div className="w-full bg-gray-700 rounded-full h-2.5">
                <div
                  className="bg-indigo-500 h-2.5 rounded-full"
                  style={{ width: `${progresso}%` }}
                ></div>
              </div>
              <p className="text-indigo-300 text-sm font-semibold mt-1 text-right">
                {ultimoCapituloLido} /{" "}
                {totalCapitulos > 0 ? totalCapitulos : "?"}
              </p>
            </div>

            {/* Data de Atualização */}
            <p className="text-gray-400 text-xs text-right mt-2">
              Atualizado em: {dataFormatada}
            </p>
          </div>
        </div>
      </div>
    </Link>
  );
}

export default MangaItem;
