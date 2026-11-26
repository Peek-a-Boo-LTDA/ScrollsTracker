import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useEffect, useRef } from "react";
import { apiFetch } from "../api";
import { chaveSessao } from "../useSessao";
import { Link, NavLink } from "react-router-dom";

import logo from "../assets/logo.png";

function Navbar({ sessao }) {
  const menuConta = useRef(null);
  const queryClient = useQueryClient();
  const partes = sessao.email.split("@")[0].split(/[._+-]+/).filter(Boolean);
  const iniciais = (partes.length > 1
    ? partes.slice(0, 2).map((parte) => parte[0]).join("")
    : partes[0]?.slice(0, 2) || "U").toLocaleUpperCase("pt-BR");

  useEffect(() => {
    function fecharAoClicarFora(evento) {
      const menu = menuConta.current;
      if (menu?.open && !menu.contains(evento.target)) menu.open = false;
    }

    document.addEventListener("pointerdown", fecharAoClicarFora);
    return () => document.removeEventListener("pointerdown", fecharAoClicarFora);
  }, []);

  const logout = useMutation({
    mutationFn: async () => {
      const resposta = await apiFetch("/api/conta/logout", { method: "POST" });
      if (!resposta.ok && resposta.status !== 401) throw new Error("Falha ao sair.");
    },
    onSuccess: async () => {
      await queryClient.cancelQueries();
      queryClient.setQueryData(chaveSessao, null);
      queryClient.removeQueries({ predicate: (query) => query.queryKey[0] !== chaveSessao[0] });
    },
  });
  return (
    <header className="app-header">
      <div className="page-width flex flex-wrap items-center justify-between gap-x-6 gap-y-3 py-3">
        <Link to="/" className="flex items-center gap-3 font-semibold tracking-tight text-neutral-100">
          <img src={logo} alt="" className="size-9 rounded-lg bg-neutral-200 object-contain p-0.5" />
          <span>ScrollsTracker</span>
        </Link>
        <nav aria-label="Navegação principal" className="order-last w-full sm:order-none sm:ml-auto sm:w-auto">
          <ul className="flex flex-wrap items-center gap-1 sm:gap-2">
            <li><NavLink end to="/" className="nav-item">Lançamentos</NavLink></li>
            <li><NavLink to="/biblioteca" className="nav-item">Biblioteca</NavLink></li>
            <li><NavLink to="/importar" className="nav-item">Importar</NavLink></li>
          </ul>
        </nav>
        <details
          ref={menuConta}
          className="relative ml-auto sm:ml-0"
          onKeyDown={(evento) => {
            if (evento.key !== "Escape" || !evento.currentTarget.open) return;
            const menu = evento.currentTarget;
            menu.open = false;
            menu.querySelector("summary").focus();
          }}
          onBlur={(evento) => {
            if (evento.relatedTarget && !evento.currentTarget.contains(evento.relatedTarget)) {
              evento.currentTarget.open = false;
            }
          }}
        >
          <summary
            aria-label="Menu da conta"
            className="grid size-10 cursor-pointer list-none place-items-center rounded-full border border-neutral-500 bg-neutral-200 text-sm font-bold text-neutral-950 hover:bg-white focus-visible:outline-2 focus-visible:outline-offset-4 focus-visible:outline-white [&::-webkit-details-marker]:hidden"
          >
            <span aria-hidden="true">{iniciais}</span>
          </summary>
          <div className="surface absolute right-0 top-full z-30 mt-2 w-64 max-w-[calc(100vw-2rem)] p-3">
            <div className="border-b border-neutral-700 px-2 pb-3">
              <p className="break-all text-sm font-semibold text-neutral-100">{sessao.email}</p>
              <p className="mt-1 text-xs text-neutral-300">
                {sessao.podeGerenciarObras ? "Administrador" : "Conta comum"}
              </p>
            </div>
            {sessao.podeGerenciarObras && (
              <NavLink to="/diagnostico-fontes" className="nav-item mt-2 w-full"
                onClick={() => { menuConta.current.open = false; }}>
                Diagnóstico das fontes
              </NavLink>
            )}
            <button
              type="button"
              onClick={() => logout.mutate()}
              disabled={logout.isPending}
              className="nav-item mt-2 w-full cursor-pointer disabled:cursor-wait disabled:opacity-60"
            >
              {logout.isPending ? "Saindo..." : "Sair"}
            </button>
            {logout.isError && <p role="alert" className="mt-2 px-2 text-sm text-red-300">Falha ao sair. Tente novamente.</p>}
          </div>
        </details>
      </div>
    </header>
  );
}

export default Navbar;
