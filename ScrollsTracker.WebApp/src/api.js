const baseUrl = import.meta.env.VITE_API_BASE_URL || "";

export async function apiFetch(path, options = {}) {
  const headers = new Headers(options.headers);
  const method = options.method || "GET";
  if (!["GET", "HEAD", "OPTIONS"].includes(method)) {
    const tokenResponse = await fetch(`${baseUrl}/api/conta/csrf`, {
      credentials: "include",
      cache: "no-store",
    });
    if (!tokenResponse.ok) throw new Error("Não foi possível iniciar a solicitação.");
    const { token } = await tokenResponse.json();
    headers.set("X-CSRF-TOKEN", token);
  }

  return fetch(`${baseUrl}${path}`, { ...options, headers, credentials: "include" });
}
