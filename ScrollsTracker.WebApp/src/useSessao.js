import { useQuery } from "@tanstack/react-query";
import { apiFetch } from "./api";

export const chaveSessao = ["sessao"];

export function useSessao() {
  return useQuery({
    queryKey: chaveSessao,
    queryFn: async () => {
      const resposta = await apiFetch("/api/conta/sessao");
      if (resposta.status === 401) return null;
      if (!resposta.ok) throw new Error("Não foi possível verificar sua sessão.");
      return resposta.json();
    },
    retry: false,
  });
}
