import { Link } from "react-router-dom";

function StatusBadge({ text }) {
  const cores = {
    Lendo: "bg-blue-500 text-blue-100",
    Completo: "bg-green-500 text-green-100",
    Pausado: "bg-yellow-500 text-yellow-100",
    "Planejo Ler": "bg-gray-500 text-gray-100",
    Abandonado: "bg-red-500 text-red-100",
    "Em Andamento": "bg-teal-500 text-teal-100",
    Finalizado: "bg-purple-500 text-purple-100",
  };

  return <span className={`rounded-full px-2 py-1 text-xs font-semibold ${cores[text] ?? "bg-gray-600 text-gray-100"}`}>{text}</span>;
}

function numeroCapitulo(valor) {
  const numero = Number(String(valor ?? "").replace(",", "."));
  return Number.isFinite(numero) && numero >= 0 ? numero : 0;
}

function MangaItem({ obra, onMarcar, disabled }) {
  const { id, titulo, descricao, imagem, totalCapitulos, ultimoCapituloLido, status,
    statusLeitura, dataAtualizacao, lancamentoRecente } = obra;
  const disponivel = lancamentoRecente?.numero ?? totalCapitulos;
  const total = numeroCapitulo(disponivel);
  const lido = numeroCapitulo(ultimoCapituloLido);
  const percentual = total > 0 ? Math.min(lido / total * 100, 100) : 0;
  const proximo = total > 0 ? Math.min(Math.floor(lido) + 1, total) : Math.floor(lido) + 1;
  const avancar = proximo > lido;

  return (
    <article className="surface manga-card flex min-w-0 flex-col gap-3 p-4">
      <Link to={`/atualizar-obra/${id}`} className="manga-link flex flex-1 gap-4 rounded-xl">
        <div className="w-24 shrink-0 sm:w-28">
          {imagem && <img src={imagem} alt={`Capa de ${titulo}`} loading="lazy" decoding="async"
            className="aspect-[2/3] w-full rounded-lg bg-slate-800 object-cover" />}
        </div>
        <div className="flex min-w-0 flex-1 flex-col justify-between wrap-anywhere">
          <div>
            <h2 className="mb-2 line-clamp-2 text-lg font-semibold text-slate-100">{titulo}</h2>
            <div className="mb-3 flex flex-wrap gap-2">
              {status && <StatusBadge text={status} />}
              {statusLeitura && <StatusBadge text={statusLeitura} />}
            </div>
          </div>
          <p className="mb-3 line-clamp-3 text-sm leading-relaxed text-slate-300">{descricao}</p>
          <div>
            <div className="h-1.5 w-full overflow-hidden rounded-full bg-slate-700">
              <div className="h-full rounded-full bg-indigo-400" style={{ width: `${percentual}%` }} />
            </div>
            <p className="mt-1 text-right text-sm font-semibold text-indigo-300">
              {ultimoCapituloLido ?? "0"} / {disponivel || "?"}
            </p>
            <p className="mt-2 text-right text-xs text-gray-400">
              {lancamentoRecente
                ? `Capítulo ${disponivel} detectado em ${new Date(lancamentoRecente.detectadoEm).toLocaleDateString("pt-BR")}`
                : `Atualizado em ${new Date(dataAtualizacao).toLocaleDateString("pt-BR")}`}
            </p>
          </div>
        </div>
      </Link>
      {avancar && (
        <div className="flex flex-wrap justify-end gap-2 border-t border-slate-700/60 pt-3">
          <button type="button" className="button button-secondary" disabled={disabled}
            aria-label={`Marcar próximo capítulo de ${titulo}`}
            onClick={() => onMarcar(obra, proximo === total ? disponivel : String(proximo))}>
            Marcar próximo
          </button>
          {total > proximo && <button type="button" className="button button-secondary" disabled={disabled}
            aria-label={`Marcar até o capítulo ${disponivel} de ${titulo}`} onClick={() => onMarcar(obra, disponivel)}>
            Marcar até {disponivel}
          </button>}
        </div>
      )}
    </article>
  );
}

export default MangaItem;
