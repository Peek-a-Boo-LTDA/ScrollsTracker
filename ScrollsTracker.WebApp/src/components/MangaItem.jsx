import { Link } from "react-router-dom";

function StatusBadge({ text }) {
  const destaque = text === "Lendo" || text === "Completo";
  return (
    <span className={`rounded-full border px-2 py-1 text-xs font-semibold ${
      destaque
        ? "border-neutral-200 bg-neutral-200 text-neutral-950"
        : "border-neutral-600 bg-neutral-900 text-neutral-200"
    }`}>
      {text}
    </span>
  );
}

function numeroCapitulo(valor) {
  const numero = Number(String(valor ?? "").replace(",", "."));
  return Number.isFinite(numero) && numero >= 0 ? numero : 0;
}

function MangaItem({ obra, onMarcar, onSeguir, disabled, mostrarPendentes }) {
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
            className="aspect-[2/3] w-full rounded-lg bg-neutral-800 object-cover" />}
        </div>
        <div className="flex min-w-0 flex-1 flex-col justify-between wrap-anywhere">
          <div>
            <h2 className="mb-2 line-clamp-2 text-lg font-semibold text-neutral-100">{titulo}</h2>
            <div className="mb-3 flex flex-wrap gap-2">
              {status && <StatusBadge text={status} />}
              {statusLeitura && <StatusBadge text={statusLeitura} />}
            </div>
          </div>
          <p className="mb-3 line-clamp-3 text-sm leading-relaxed text-neutral-300">{descricao}</p>
          <div>
            <div className="h-1.5 w-full overflow-hidden rounded-full bg-neutral-700">
              <div className="h-full rounded-full bg-neutral-200" style={{ width: `${percentual}%` }} />
            </div>
            <p className="mt-1 text-right text-sm font-semibold text-neutral-100">
              {ultimoCapituloLido ?? "0"} / {disponivel || "?"}
            </p>
            {mostrarPendentes && total > lido && Number.isInteger(total) && Number.isInteger(lido) && (
              <p className="text-right text-xs text-neutral-300">
                {total - lido === 1 ? "Falta 1 capítulo" : `Faltam ${total - lido} capítulos`}
              </p>
            )}
            <p className="mt-2 text-right text-xs text-gray-400">
              {lancamentoRecente
                ? `Capítulo ${disponivel} detectado em ${new Date(lancamentoRecente.detectadoEm).toLocaleDateString("pt-BR")}`
                : `Atualizado em ${new Date(dataAtualizacao).toLocaleDateString("pt-BR")}`}
            </p>
          </div>
        </div>
      </Link>
      <div className="flex flex-wrap justify-end gap-2 border-t border-neutral-700/60 pt-3">
        <button type="button" className="button button-secondary mr-auto" disabled={disabled}
          aria-label={`Seguir ${titulo}`} aria-pressed={obra.seguindo === true}
          onClick={() => onSeguir(obra)}>
          {obra.seguindo ? "Seguindo" : "Seguir"}
        </button>
        {avancar && (
          <>
          <button type="button" className="button button-secondary" disabled={disabled}
            aria-label={`Marcar próximo capítulo de ${titulo}`}
            onClick={() => onMarcar(obra, proximo === total ? disponivel : String(proximo))}>
            Marcar próximo
          </button>
          {total > proximo && <button type="button" className="button button-secondary" disabled={disabled}
            aria-label={`Marcar até o capítulo ${disponivel} de ${titulo}`} onClick={() => onMarcar(obra, disponivel)}>
            Marcar até {disponivel}
          </button>}
          </>
        )}
      </div>
    </article>
  );
}

export default MangaItem;
