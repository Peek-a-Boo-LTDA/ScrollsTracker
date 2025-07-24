import MangaItem from "../components/MangaItem.jsx";

function MangaShelf(props) {
  const { obras } = props;
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 p-4">
      {obras.map((obra) => (
        <MangaItem
          id={obra.id}
          titulo={obra.titulo}
          descricao={obra.descricao}
          imagem={obra.imagem}
          totalCapitulos={obra.totalCapitulos}
          ultimoCapituloLido={obra.ultimoCapituloLido}
          status={obra.status}
          statusLeitura={obra.statusLeitura}
          dataAtualizacao={obra.dataAtualizacao}
          key={obra.id}
        />
      ))}
    </div>
  );
}

export default MangaShelf;
