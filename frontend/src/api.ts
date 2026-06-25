import type {
  AuthResponseDto,
  AuthUser,
  CommentItem,
  PagedResponse,
  Project,
  TaskItem,
  TaskPriority,
  TaskStatus,
  UserRole
} from "./types";

const configuredBaseUrl = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5100/api";
export const API_BASE_URL = configuredBaseUrl.replace(/\/$/, "");

type TokenPayload = {
  nameid?: string;
  sub?: string;
  role?: string;
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"?: string;
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"?: string;
};

export class ApiError extends Error {
  status: number;

  constructor(message: string, status: number) {
    super(message);
    this.status = status;
  }
}

export function parseJwt(token: string): TokenPayload {
  try {
    const [, payload] = token.split(".");
    const normalized = payload.replace(/-/g, "+").replace(/_/g, "/");
    return JSON.parse(window.atob(normalized));
  } catch {
    return {};
  }
}

export function authUserFromResponse(response: AuthResponseDto): AuthUser {
  const payload = parseJwt(response.token);
  const id =
    Number(payload.nameid) ||
    Number(payload.sub) ||
    Number(payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"]) ||
    null;
  const roleClaim = payload.role ?? payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
  const roleFromToken = roleClaim === "Admin" ? 1 : roleClaim === "Manager" ? 2 : roleClaim === "Developer" ? 3 : null;

  return {
    ...response,
    id,
    role: (response.role || roleFromToken || 3) as UserRole
  };
}

async function readError(response: Response): Promise<string> {
  const contentType = response.headers.get("content-type") ?? "";
  if (contentType.includes("application/json")) {
    const body = await response.json();
    if (typeof body === "string") return body;
    if (body?.message) return body.message;
    if (body?.title) return body.title;
    if (body?.errors) return Object.values(body.errors).flat().join(" ");
  }

  const text = await response.text();
  return text || `Request failed with status ${response.status}`;
}

async function request<T>(path: string, token?: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...init?.headers
    }
  });

  if (!response.ok) {
    throw new ApiError(await readError(response), response.status);
  }

  if (response.status === 204) return undefined as T;
  return response.json() as Promise<T>;
}

export const api = {
  register: (payload: { fullName: string; email: string; password: string }) =>
    request<AuthResponseDto>("/Auth/register", undefined, {
      method: "POST",
      body: JSON.stringify(payload)
    }),

  login: (payload: { email: string; password: string }) =>
    request<AuthResponseDto>("/Auth/login", undefined, {
      method: "POST",
      body: JSON.stringify(payload)
    }),

  projects: (token: string, pageNumber = 1, pageSize = 6) =>
    request<PagedResponse<Project>>(`/Project?pageNumber=${pageNumber}&pageSize=${pageSize}`, token),

  project: (token: string, id: number) => request<Project>(`/Project/${id}`, token),

  createProject: (token: string, payload: { name: string; description: string }) =>
    request<Project>("/Project", token, {
      method: "POST",
      body: JSON.stringify(payload)
    }),

  updateProject: (token: string, id: number, payload: { name: string; description: string }) =>
    request<Project>(`/Project/${id}`, token, {
      method: "PUT",
      body: JSON.stringify(payload)
    }),

  completeProject: (token: string, id: number) =>
    request<Project>(`/Project/${id}/complete`, token, { method: "PATCH" }),

  archiveProject: (token: string, id: number) =>
    request<Project>(`/Project/${id}/archive`, token, { method: "PATCH" }),

  deleteProject: (token: string, id: number) =>
    request<void>(`/Project/${id}`, token, { method: "DELETE" }),

  tasks: (token: string, pageNumber = 1, pageSize = 8) =>
    request<PagedResponse<TaskItem>>(`/tasks?pageNumber=${pageNumber}&pageSize=${pageSize}`, token),

  myTasks: (token: string, pageNumber = 1, pageSize = 8) =>
    request<PagedResponse<TaskItem>>(`/tasks/my?pageNumber=${pageNumber}&pageSize=${pageSize}`, token),

  tasksByProject: (token: string, projectId: number, pageNumber = 1, pageSize = 8) =>
    request<PagedResponse<TaskItem>>(`/projects/${projectId}/tasks?pageNumber=${pageNumber}&pageSize=${pageSize}`, token),

  task: (token: string, id: number) => request<TaskItem>(`/tasks/${id}`, token),

  createTask: (
    token: string,
    payload: {
      title: string;
      description: string;
      dueDate: string | null;
      priority: TaskPriority;
      projectId: number;
      assignedUserId: number;
    }
  ) =>
    request<TaskItem>("/tasks", token, {
      method: "POST",
      body: JSON.stringify(payload)
    }),

  updateTask: (
    token: string,
    id: number,
    payload: { title: string; description: string; priority: TaskPriority; dueDate: string | null }
  ) =>
    request<TaskItem>(`/tasks/${id}`, token, {
      method: "PUT",
      body: JSON.stringify(payload)
    }),

  updateTaskStatus: (token: string, id: number, status: TaskStatus) =>
    request<TaskItem>(`/tasks/${id}/status`, token, {
      method: "PATCH",
      body: JSON.stringify({ status })
    }),

  assignTask: (token: string, id: number, assignedUserId: number | null) =>
    request<TaskItem>(`/tasks/${id}/assign`, token, {
      method: "PATCH",
      body: JSON.stringify({ assignedUserId })
    }),

  deleteTask: (token: string, id: number) =>
    request<void>(`/tasks/${id}`, token, { method: "DELETE" }),

  comments: (token: string, taskId: number) =>
    request<CommentItem[]>(`/Comment/tasks/${taskId}/comments`, token),

  addComment: (token: string, taskId: number, text: string) =>
    request<CommentItem>(`/Comment/tasks/${taskId}/comments`, token, {
      method: "POST",
      body: JSON.stringify({ text })
    }),

  deleteComment: (token: string, id: number) =>
    request<void>(`/Comment/comments/${id}`, token, { method: "DELETE" })
};
