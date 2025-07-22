import { Link } from "react-router-dom";

function MangaItem(props) {
  // Desestruturar as props para um código mais limpo é uma boa prática.
  const { id, titulo, descricao, imagem, ultimoCapitulo } = props;

  return (
    <Link to={`/atualizar-obra/${id}`}>
      <div className="flex gap-6 p-4 m-3 bg-stone-900 rounded-lg shadow-md border border-indigo-900">
        <div className="w-24 md:w-32 flex-shrink-0">
          <img
            src={imagem}
            alt={`Capa de ${titulo}`}
            className="w-full h-full object-cover rounded-md"
          />
        </div>
        <div className="flex flex-col justify-center">
          <h2 className="text-xl font-bold text-white mb-1">{titulo}</h2>
          <p className="text-white text-sm line-clamp-3 mb-1">{descricao}</p>
          <p className="text-white text-xl line-clamp-3">
            Ultimo Capítulo: {ultimoCapitulo}
          </p>
        </div>
      </div>
    </Link>
  );
}

export default MangaItem;
