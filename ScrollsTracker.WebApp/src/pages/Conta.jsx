import { useMutation, useQueryClient } from "@tanstack/react-query";
import { Link, Navigate } from "react-router-dom";
import { apiFetch } from "../api";
import { chaveSessao, useSessao } from "../useSessao";

function Conta({ cadastro = false }) {
  const sessao = useSessao();
  const queryClient = useQueryClient();
  const acesso = useMutation({
    mutationFn: async (credenciais) => {
      const resposta = await apiFetch(cadastro ? "/api/conta/cadastro" : "/api/conta/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(credenciais),
      });

      if (resposta.status === 429) {
        throw new Error("Muitas tentativas. Aguarde um minuto e tente novamente.");
      }
      if (!resposta.ok) {
        const erro = await resposta.json().catch(() => null);
        throw new Error(erro?.mensagem || "Confira os dados e tente novamente.");
      }
    },
    onSuccess: async () => {
      queryClient.removeQueries({ predicate: (query) => query.queryKey[0] !== chaveSessao[0] });
      await queryClient.invalidateQueries({ queryKey: chaveSessao });
    },
  });

  if (sessao.data) return <Navigate to="/biblioteca" replace />;

  const enviar = (event) => {
    event.preventDefault();
    const dados = new FormData(event.currentTarget);
    acesso.mutate({ email: dados.get("email"), senha: dados.get("senha") });
  };

  return (
    <main className="account-page">
      <form onSubmit={enviar} className="surface mx-auto grid w-full max-w-sm gap-5 p-6 sm:p-8">
        <h1 className="page-title">{cadastro ? "Criar conta" : "Entrar no ScrollsTracker"}</h1>
        <label className="grid gap-2">
          E-mail
          <input name="email" type="email" autoComplete="email" required maxLength={256} className="field w-full" />
        </label>
        <label className="grid gap-2">
          Senha
          <input name="senha" type="password" autoComplete={cadastro ? "new-password" : "current-password"}
            required minLength={cadastro ? 8 : undefined} maxLength={128} className="field w-full" />
        </label>
        {cadastro && <p className="text-sm text-gray-300">Use ao menos 8 caracteres, com maiúscula, minúscula, número e símbolo.</p>}
        {acesso.isError && <p role="alert" className="text-red-300">{acesso.error.message}</p>}
        <button type="submit" disabled={acesso.isPending} className="button button-primary">
          {cadastro ? "Cadastrar" : "Entrar"}
        </button>
        <Link to={cadastro ? "/login" : "/cadastro"} className="text-neutral-200 underline">
          {cadastro ? "Já tenho conta" : "Criar conta"}
        </Link>
      </form>
    </main>
  );
}

export default Conta;
