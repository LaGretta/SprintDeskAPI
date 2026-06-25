import { FormEvent, useEffect, useMemo, useState } from "react";
import {
  Archive,
  CheckCircle2,
  ClipboardList,
  Loader2,
  LogOut,
  MessageSquare,
  Plus,
  RefreshCw,
  Save,
  Send,
  Shield,
  Trash2,
  UserPlus,
  Users
} from "lucide-react";
import { API_BASE_URL, ApiError, api, authUserFromResponse } from "./api";
import {
  AuthUser,
  CommentItem,
  PagedResponse,
  Project,
  TaskItem,
  TaskPriority,
  TaskStatus,
  projectStatusLabels,
  roleLabels,
  taskPriorityLabels,
  taskPriorityOptions,
  taskStatusLabels,
  taskStatusOptions
} from "./types";

const storageKey = "sprintdesk.auth";

const emptyProjects: PagedResponse<Project> = {
  items: [],
  pageNumber: 1,
  pageSize: 6,
  totalItems: 0,
  totalPages: 0
};

const emptyTasks: PagedResponse<TaskItem> = {
  items: [],
  pageNumber: 1,
  pageSize: 8,
  totalItems: 0,
  totalPages: 0
};

type View = "overview" | "projects" | "tasks" | "my";
type Toast = { kind: "success" | "error"; text: string } | null;

function getStoredUser(): AuthUser | null {
  const raw = localStorage.getItem(storageKey);
  if (!raw) return null;

  try {
    return JSON.parse(raw) as AuthUser;
  } catch {
    localStorage.removeItem(storageKey);
    return null;
  }
}

function formatDate(value: string | null) {
  if (!value) return "No date";
  return new Intl.DateTimeFormat(undefined, { month: "short", day: "numeric", year: "numeric" }).format(new Date(value));
}

function dueDateInputValue(value: string | null) {
  return value ? value.slice(0, 10) : "";
}

function dateFromInput(value: string) {
  return value ? `${value}T12:00:00.000Z` : null;
}

function messageFromError(error: unknown) {
  if (error instanceof ApiError) {
    if (error.status === 401) return "Your session is missing or expired. Please log in again.";
    if (error.status === 403) return "Your role cannot perform this action.";
    return error.message;
  }

  return error instanceof Error ? error.message : "Something went wrong.";
}

export function App() {
  const [user, setUser] = useState<AuthUser | null>(() => getStoredUser());
  const [view, setView] = useState<View>("overview");
  const [toast, setToast] = useState<Toast>(null);

  const [projects, setProjects] = useState<PagedResponse<Project>>(emptyProjects);
  const [tasks, setTasks] = useState<PagedResponse<TaskItem>>(emptyTasks);
  const [myTasks, setMyTasks] = useState<PagedResponse<TaskItem>>(emptyTasks);
  const [projectTasks, setProjectTasks] = useState<PagedResponse<TaskItem>>(emptyTasks);

  const [selectedProject, setSelectedProject] = useState<Project | null>(null);
  const [selectedTask, setSelectedTask] = useState<TaskItem | null>(null);
  const [comments, setComments] = useState<CommentItem[]>([]);

  const [loading, setLoading] = useState(false);
  const [detailLoading, setDetailLoading] = useState(false);
  const [commentsLoading, setCommentsLoading] = useState(false);

  const canManage = user?.role === 1 || user?.role === 2;
  const isAdmin = user?.role === 1;

  const showToast = (nextToast: Toast) => {
    setToast(nextToast);
    window.setTimeout(() => setToast(null), 3800);
  };

  const run = async <T,>(action: () => Promise<T>, success?: string) => {
    try {
      const result = await action();
      if (success) showToast({ kind: "success", text: success });
      return result;
    } catch (error) {
      showToast({ kind: "error", text: messageFromError(error) });
      throw error;
    }
  };

  const handleAuth = (nextUser: AuthUser) => {
    setUser(nextUser);
    localStorage.setItem(storageKey, JSON.stringify(nextUser));
    setView("overview");
    showToast({ kind: "success", text: "Signed in successfully." });
  };

  const logout = () => {
    setUser(null);
    localStorage.removeItem(storageKey);
    setSelectedProject(null);
    setSelectedTask(null);
    setComments([]);
  };

  const loadProjects = async (pageNumber = projects.pageNumber || 1) => {
    if (!user) return;
    setLoading(true);
    try {
      const nextProjects = await run(() => api.projects(user.token, pageNumber, 6));
      setProjects(nextProjects);
      if (!selectedProject && nextProjects.items.length > 0) {
        setSelectedProject(nextProjects.items[0]);
      }
    } finally {
      setLoading(false);
    }
  };

  const loadTasks = async (pageNumber = tasks.pageNumber || 1) => {
    if (!user) return;
    setLoading(true);
    try {
      setTasks(await run(() => api.tasks(user.token, pageNumber, 8)));
    } finally {
      setLoading(false);
    }
  };

  const loadMyTasks = async (pageNumber = myTasks.pageNumber || 1) => {
    if (!user) return;
    setLoading(true);
    try {
      setMyTasks(await run(() => api.myTasks(user.token, pageNumber, 8)));
    } finally {
      setLoading(false);
    }
  };

  const loadProjectTasks = async (projectId: number, pageNumber = 1) => {
    if (!user) return;
    setDetailLoading(true);
    try {
      setProjectTasks(await run(() => api.tasksByProject(user.token, projectId, pageNumber, 8)));
    } finally {
      setDetailLoading(false);
    }
  };

  const loadComments = async (taskId: number) => {
    if (!user) return;
    setCommentsLoading(true);
    try {
      setComments(await run(() => api.comments(user.token, taskId)));
    } finally {
      setCommentsLoading(false);
    }
  };

  useEffect(() => {
    if (!user) return;
    void loadProjects(1);
    void loadTasks(1);
    void loadMyTasks(1);
  }, [user]);

  useEffect(() => {
    if (!selectedProject) return;
    void loadProjectTasks(selectedProject.id, 1);
  }, [selectedProject?.id]);

  useEffect(() => {
    if (!selectedTask) {
      setComments([]);
      return;
    }
    void loadComments(selectedTask.id);
  }, [selectedTask?.id]);

  const stats = useMemo(
    () => ({
      projects: projects.totalItems,
      tasks: tasks.totalItems,
      mine: myTasks.totalItems,
      activeProjects: projects.items.filter((project) => project.status === 1).length
    }),
    [projects, tasks, myTasks]
  );

  if (!user) {
    return (
      <AuthScreen
        onAuth={handleAuth}
        toast={toast}
        showToast={showToast}
      />
    );
  }

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand">
          <div className="brand-mark">S</div>
          <div>
            <strong>SprintDesk</strong>
            <span>Team operations</span>
          </div>
        </div>

        <nav className="nav-list">
          <button className={view === "overview" ? "active" : ""} onClick={() => setView("overview")}>
            <ClipboardList size={18} /> Overview
          </button>
          <button className={view === "projects" ? "active" : ""} onClick={() => setView("projects")}>
            <Users size={18} /> Projects
          </button>
          <button className={view === "tasks" ? "active" : ""} onClick={() => setView("tasks")}>
            <CheckCircle2 size={18} /> Tasks
          </button>
          <button className={view === "my" ? "active" : ""} onClick={() => setView("my")}>
            <Shield size={18} /> My tasks
          </button>
        </nav>

        <div className="profile-card">
          <span className="avatar">{user.fullName?.charAt(0) || user.email.charAt(0)}</span>
          <div>
            <strong>{user.fullName || user.email}</strong>
            <small>{roleLabels[user.role]}</small>
          </div>
        </div>

        <button className="ghost-button" onClick={logout}>
          <LogOut size={17} /> Logout
        </button>
      </aside>

      <main className="workspace">
        <header className="topbar">
          <div>
            <span className="eyebrow">API</span>
            <h1>{view === "overview" ? "Workspace overview" : view === "projects" ? "Projects" : view === "tasks" ? "Tasks" : "My tasks"}</h1>
          </div>
          <div className="api-chip">{API_BASE_URL}</div>
        </header>

        {toast && <div className={`toast ${toast.kind}`}>{toast.text}</div>}

        {view === "overview" && (
          <Overview
            stats={stats}
            loading={loading}
            recentProjects={projects.items}
            recentTasks={tasks.items}
            onOpenProjects={() => setView("projects")}
            onOpenTasks={() => setView("tasks")}
          />
        )}

        {view === "projects" && (
          <ProjectsView
            user={user}
            canManage={canManage}
            isAdmin={isAdmin}
            loading={loading}
            detailLoading={detailLoading}
            projects={projects}
            selectedProject={selectedProject}
            projectTasks={projectTasks}
            onPage={loadProjects}
            onSelect={setSelectedProject}
            onReload={() => loadProjects(projects.pageNumber)}
            onReloadProjectTasks={(page) => selectedProject && loadProjectTasks(selectedProject.id, page)}
            onSuccess={showToast}
            setProjects={setProjects}
            setSelectedProject={setSelectedProject}
            setSelectedTask={setSelectedTask}
          />
        )}

        {(view === "tasks" || view === "my") && (
          <TasksView
            user={user}
            canManage={canManage}
            loading={loading}
            paged={view === "tasks" ? tasks : myTasks}
            projects={projects.items}
            selectedTask={selectedTask}
            comments={comments}
            commentsLoading={commentsLoading}
            onPage={view === "tasks" ? loadTasks : loadMyTasks}
            onReload={view === "tasks" ? () => loadTasks(tasks.pageNumber) : () => loadMyTasks(myTasks.pageNumber)}
            onSelectTask={setSelectedTask}
            onReloadComments={() => selectedTask && loadComments(selectedTask.id)}
            onSuccess={showToast}
            setSelectedTask={setSelectedTask}
            refreshAll={() => {
              void loadTasks(tasks.pageNumber);
              void loadMyTasks(myTasks.pageNumber);
              if (selectedProject) void loadProjectTasks(selectedProject.id, projectTasks.pageNumber || 1);
            }}
          />
        )}
      </main>
    </div>
  );
}

function AuthScreen({
  onAuth,
  toast,
  showToast
}: {
  onAuth: (user: AuthUser) => void;
  toast: Toast;
  showToast: (toast: Toast) => void;
}) {
  const [mode, setMode] = useState<"login" | "register">("login");
  const [loading, setLoading] = useState(false);
  const [form, setForm] = useState({ fullName: "", email: "", password: "" });

  const submit = async (event: FormEvent) => {
    event.preventDefault();
    setLoading(true);

    try {
      const response =
        mode === "login"
          ? await api.login({ email: form.email, password: form.password })
          : await api.register({ fullName: form.fullName, email: form.email, password: form.password });
      onAuth(authUserFromResponse(response));
    } catch (error) {
      showToast({ kind: "error", text: messageFromError(error) });
    } finally {
      setLoading(false);
    }
  };

  return (
    <main className="auth-page">
      <section className="auth-visual">
        <div className="brand large">
          <div className="brand-mark">S</div>
          <div>
            <strong>SprintDesk</strong>
            <span>Projects, tasks, comments, roles</span>
          </div>
        </div>
        <div className="metric-strip">
          <span>JWT</span>
          <span>Roles</span>
          <span>Pagination</span>
        </div>
      </section>

      <section className="auth-panel">
        {toast && <div className={`toast inline ${toast.kind}`}>{toast.text}</div>}
        <div className="segmented">
          <button className={mode === "login" ? "active" : ""} onClick={() => setMode("login")}>Login</button>
          <button className={mode === "register" ? "active" : ""} onClick={() => setMode("register")}>Register</button>
        </div>

        <form onSubmit={submit} className="form-stack">
          {mode === "register" && (
            <label>
              Full name
              <input value={form.fullName} onChange={(event) => setForm({ ...form, fullName: event.target.value })} required minLength={3} />
            </label>
          )}
          <label>
            Email
            <input type="email" value={form.email} onChange={(event) => setForm({ ...form, email: event.target.value })} required />
          </label>
          <label>
            Password
            <input type="password" value={form.password} onChange={(event) => setForm({ ...form, password: event.target.value })} required minLength={6} />
          </label>
          <button className="primary-button" disabled={loading}>
            {loading ? <Loader2 className="spin" size={18} /> : <Shield size={18} />}
            {mode === "login" ? "Login" : "Create account"}
          </button>
        </form>
      </section>
    </main>
  );
}

function Overview({
  stats,
  loading,
  recentProjects,
  recentTasks,
  onOpenProjects,
  onOpenTasks
}: {
  stats: { projects: number; tasks: number; mine: number; activeProjects: number };
  loading: boolean;
  recentProjects: Project[];
  recentTasks: TaskItem[];
  onOpenProjects: () => void;
  onOpenTasks: () => void;
}) {
  return (
    <div className="content-grid">
      <section className="stats-grid">
        <Stat label="Projects" value={stats.projects} />
        <Stat label="Active" value={stats.activeProjects} />
        <Stat label="Tasks" value={stats.tasks} />
        <Stat label="Assigned to me" value={stats.mine} />
      </section>

      <section className="panel wide">
        <PanelHeader title="Recent projects" actionLabel="Open" onAction={onOpenProjects} />
        {loading ? <LoadingState /> : recentProjects.length === 0 ? <EmptyState text="No projects yet." /> : (
          <div className="compact-list">
            {recentProjects.slice(0, 5).map((project) => (
              <div className="list-row" key={project.id}>
                <div>
                  <strong>{project.name}</strong>
                  <small>{project.description}</small>
                </div>
                <StatusBadge tone={project.status === 1 ? "green" : project.status === 2 ? "blue" : "gray"}>
                  {projectStatusLabels[project.status]}
                </StatusBadge>
              </div>
            ))}
          </div>
        )}
      </section>

      <section className="panel">
        <PanelHeader title="Latest tasks" actionLabel="Open" onAction={onOpenTasks} />
        {loading ? <LoadingState /> : recentTasks.length === 0 ? <EmptyState text="No tasks yet." /> : (
          <div className="compact-list">
            {recentTasks.slice(0, 5).map((task) => (
              <div className="list-row" key={task.id}>
                <div>
                  <strong>{task.title}</strong>
                  <small>{task.projectName || "Project"}</small>
                </div>
                <StatusBadge tone={task.priority >= 3 ? "red" : "amber"}>{taskPriorityLabels[task.priority]}</StatusBadge>
              </div>
            ))}
          </div>
        )}
      </section>
    </div>
  );
}

function ProjectsView(props: {
  user: AuthUser;
  canManage: boolean;
  isAdmin: boolean;
  loading: boolean;
  detailLoading: boolean;
  projects: PagedResponse<Project>;
  selectedProject: Project | null;
  projectTasks: PagedResponse<TaskItem>;
  onPage: (page: number) => Promise<void>;
  onSelect: (project: Project) => void;
  onReload: () => Promise<void>;
  onReloadProjectTasks: (page: number) => void;
  onSuccess: (toast: Toast) => void;
  setProjects: (projects: PagedResponse<Project>) => void;
  setSelectedProject: (project: Project | null) => void;
  setSelectedTask: (task: TaskItem) => void;
}) {
  const [projectForm, setProjectForm] = useState({ name: "", description: "" });
  const [saving, setSaving] = useState(false);

  const submitProject = async (event: FormEvent) => {
    event.preventDefault();
    setSaving(true);
    try {
      const saved = props.selectedProject
        ? await api.updateProject(props.user.token, props.selectedProject.id, projectForm)
        : await api.createProject(props.user.token, projectForm);
      props.onSuccess({ kind: "success", text: props.selectedProject ? "Project updated." : "Project created." });
      setProjectForm({ name: "", description: "" });
      props.setSelectedProject(saved);
      await props.onReload();
    } catch (error) {
      props.onSuccess({ kind: "error", text: messageFromError(error) });
    } finally {
      setSaving(false);
    }
  };

  const editProject = (project: Project) => {
    props.onSelect(project);
    setProjectForm({ name: project.name, description: project.description });
  };

  const completeProject = async () => {
    if (!props.selectedProject) return;
    try {
      const saved = await api.completeProject(props.user.token, props.selectedProject.id);
      props.setSelectedProject(saved);
      props.onSuccess({ kind: "success", text: "Project completed." });
      await props.onReload();
    } catch (error) {
      props.onSuccess({ kind: "error", text: messageFromError(error) });
    }
  };

  const archiveProject = async () => {
    if (!props.selectedProject) return;
    try {
      const saved = await api.archiveProject(props.user.token, props.selectedProject.id);
      props.setSelectedProject(saved);
      props.onSuccess({ kind: "success", text: "Project archived." });
      await props.onReload();
    } catch (error) {
      props.onSuccess({ kind: "error", text: messageFromError(error) });
    }
  };

  const deleteProject = async () => {
    if (!props.selectedProject) return;
    try {
      await api.deleteProject(props.user.token, props.selectedProject.id);
      props.setSelectedProject(null);
      props.onSuccess({ kind: "success", text: "Project deleted." });
      await props.onReload();
    } catch (error) {
      props.onSuccess({ kind: "error", text: messageFromError(error) });
    }
  };

  return (
    <div className="two-column">
      <section className="panel">
        <PanelHeader title="Project list" icon={<RefreshCw size={17} />} onIconAction={props.onReload} />
        {props.loading ? <LoadingState /> : props.projects.items.length === 0 ? <EmptyState text="No projects found." /> : (
          <div className="entity-list">
            {props.projects.items.map((project) => (
              <button
                className={`entity-button ${props.selectedProject?.id === project.id ? "selected" : ""}`}
                key={project.id}
                onClick={() => props.onSelect(project)}
              >
                <span>
                  <strong>{project.name}</strong>
                  <small>{project.description}</small>
                </span>
                <StatusBadge tone={project.status === 1 ? "green" : project.status === 2 ? "blue" : "gray"}>
                  {projectStatusLabels[project.status]}
                </StatusBadge>
              </button>
            ))}
          </div>
        )}
        <Pagination paged={props.projects} onPage={props.onPage} />
      </section>

      <section className="panel detail-panel">
        <PanelHeader title={props.selectedProject?.name ?? "Project details"} />
        {!props.selectedProject ? <EmptyState text="Select a project." /> : (
          <>
            <p className="description">{props.selectedProject.description}</p>
            <div className="meta-grid">
              <Meta label="Status" value={projectStatusLabels[props.selectedProject.status]} />
              <Meta label="Created" value={formatDate(props.selectedProject.createdAt)} />
              <Meta label="Updated" value={formatDate(props.selectedProject.updatedAt)} />
            </div>

            {props.canManage && (
              <div className="button-row">
                <button className="secondary-button" onClick={() => editProject(props.selectedProject!)}>
                  <Save size={16} /> Edit
                </button>
                <button className="secondary-button" onClick={completeProject}>
                  <CheckCircle2 size={16} /> Complete
                </button>
                {props.isAdmin && (
                  <>
                    <button className="secondary-button" onClick={archiveProject}>
                      <Archive size={16} /> Archive
                    </button>
                    <button className="danger-button" onClick={deleteProject}>
                      <Trash2 size={16} /> Delete
                    </button>
                  </>
                )}
              </div>
            )}

            <div className="section-title">Project tasks</div>
            {props.detailLoading ? <LoadingState /> : props.projectTasks.items.length === 0 ? <EmptyState text="No tasks in this project." /> : (
              <TaskList tasks={props.projectTasks.items} onSelect={props.setSelectedTask} />
            )}
            <Pagination paged={props.projectTasks} onPage={props.onReloadProjectTasks} />
          </>
        )}
      </section>

      {props.canManage && (
        <section className="panel form-panel">
          <PanelHeader title={props.selectedProject ? "Project form" : "New project"} />
          <form className="form-stack" onSubmit={submitProject}>
            <label>
              Name
              <input value={projectForm.name} onChange={(event) => setProjectForm({ ...projectForm, name: event.target.value })} required minLength={3} />
            </label>
            <label>
              Description
              <textarea value={projectForm.description} onChange={(event) => setProjectForm({ ...projectForm, description: event.target.value })} required minLength={3} />
            </label>
            <div className="button-row">
              <button className="primary-button" disabled={saving}>
                {saving ? <Loader2 className="spin" size={17} /> : <Plus size={17} />}
                {props.selectedProject ? "Save project" : "Create project"}
              </button>
              <button type="button" className="ghost-button compact" onClick={() => {
                props.setSelectedProject(null);
                setProjectForm({ name: "", description: "" });
              }}>
                Clear
              </button>
            </div>
          </form>
        </section>
      )}
    </div>
  );
}

function TasksView(props: {
  user: AuthUser;
  canManage: boolean;
  loading: boolean;
  paged: PagedResponse<TaskItem>;
  projects: Project[];
  selectedTask: TaskItem | null;
  comments: CommentItem[];
  commentsLoading: boolean;
  onPage: (page: number) => Promise<void>;
  onReload: () => Promise<void>;
  onSelectTask: (task: TaskItem) => void;
  onReloadComments: () => void;
  onSuccess: (toast: Toast) => void;
  setSelectedTask: (task: TaskItem | null) => void;
  refreshAll: () => void;
}) {
  const [taskForm, setTaskForm] = useState({
    title: "",
    description: "",
    priority: 2 as TaskPriority,
    dueDate: "",
    projectId: "",
    assignedUserId: ""
  });
  const [saving, setSaving] = useState(false);

  const editTask = (task: TaskItem) => {
    props.onSelectTask(task);
    setTaskForm({
      title: task.title,
      description: task.description,
      priority: task.priority,
      dueDate: dueDateInputValue(task.dueDate),
      projectId: "",
      assignedUserId: task.assignedUserId?.toString() ?? ""
    });
  };

  const submitTask = async (event: FormEvent) => {
    event.preventDefault();
    setSaving(true);
    try {
      if (props.selectedTask) {
        const saved = await api.updateTask(props.user.token, props.selectedTask.id, {
          title: taskForm.title,
          description: taskForm.description,
          priority: taskForm.priority,
          dueDate: dateFromInput(taskForm.dueDate)
        });
        props.setSelectedTask(saved);
        props.onSuccess({ kind: "success", text: "Task updated." });
      } else {
        const saved = await api.createTask(props.user.token, {
          title: taskForm.title,
          description: taskForm.description,
          priority: taskForm.priority,
          dueDate: dateFromInput(taskForm.dueDate),
          projectId: Number(taskForm.projectId),
          assignedUserId: Number(taskForm.assignedUserId)
        });
        props.setSelectedTask(saved);
        props.onSuccess({ kind: "success", text: "Task created." });
      }
      props.refreshAll();
    } catch (error) {
      props.onSuccess({ kind: "error", text: messageFromError(error) });
    } finally {
      setSaving(false);
    }
  };

  const changeStatus = async (status: TaskStatus) => {
    if (!props.selectedTask) return;
    try {
      const saved = await api.updateTaskStatus(props.user.token, props.selectedTask.id, status);
      props.setSelectedTask(saved);
      props.refreshAll();
      props.onSuccess({ kind: "success", text: "Status updated." });
    } catch (error) {
      props.onSuccess({ kind: "error", text: messageFromError(error) });
    }
  };

  const assignTask = async () => {
    if (!props.selectedTask) return;
    try {
      const saved = await api.assignTask(
        props.user.token,
        props.selectedTask.id,
        taskForm.assignedUserId ? Number(taskForm.assignedUserId) : null
      );
      props.setSelectedTask(saved);
      props.refreshAll();
      props.onSuccess({ kind: "success", text: "Assignment updated." });
    } catch (error) {
      props.onSuccess({ kind: "error", text: messageFromError(error) });
    }
  };

  const deleteTask = async () => {
    if (!props.selectedTask) return;
    try {
      await api.deleteTask(props.user.token, props.selectedTask.id);
      props.setSelectedTask(null);
      props.refreshAll();
      props.onSuccess({ kind: "success", text: "Task deleted." });
    } catch (error) {
      props.onSuccess({ kind: "error", text: messageFromError(error) });
    }
  };

  return (
    <div className="two-column">
      <section className="panel">
        <PanelHeader title="Task list" icon={<RefreshCw size={17} />} onIconAction={props.onReload} />
        {props.loading ? <LoadingState /> : props.paged.items.length === 0 ? <EmptyState text="No tasks found." /> : (
          <TaskList tasks={props.paged.items} onSelect={(task) => {
            props.onSelectTask(task);
            editTask(task);
          }} selectedTask={props.selectedTask} />
        )}
        <Pagination paged={props.paged} onPage={props.onPage} />
      </section>

      <section className="panel detail-panel">
        <PanelHeader title={props.selectedTask?.title ?? "Task details"} />
        {!props.selectedTask ? <EmptyState text="Select a task." /> : (
          <>
            <p className="description">{props.selectedTask.description}</p>
            <div className="meta-grid">
              <Meta label="Project" value={props.selectedTask.projectName || "Unknown"} />
              <Meta label="Assignee" value={props.selectedTask.assignedUserFullName || props.selectedTask.assignedUserId?.toString() || "Unassigned"} />
              <Meta label="Due" value={formatDate(props.selectedTask.dueDate)} />
              <Meta label="Priority" value={taskPriorityLabels[props.selectedTask.priority]} />
            </div>

            <label className="compact-label">
              Status
              <select value={props.selectedTask.status} onChange={(event) => changeStatus(Number(event.target.value) as TaskStatus)}>
                {taskStatusOptions.map((status) => (
                  <option key={status} value={status}>{taskStatusLabels[status]}</option>
                ))}
              </select>
            </label>

            {props.canManage && (
              <div className="assign-row">
                <label>
                  Assigned user id
                  <input type="number" value={taskForm.assignedUserId} onChange={(event) => setTaskForm({ ...taskForm, assignedUserId: event.target.value })} />
                </label>
                <button className="secondary-button" onClick={assignTask}>
                  <UserPlus size={16} /> Assign
                </button>
                <button className="danger-button" onClick={deleteTask}>
                  <Trash2 size={16} /> Delete
                </button>
              </div>
            )}

            <CommentsPanel
              user={props.user}
              task={props.selectedTask}
              comments={props.comments}
              loading={props.commentsLoading}
              onReload={props.onReloadComments}
              onSuccess={props.onSuccess}
            />
          </>
        )}
      </section>

      {props.canManage && (
        <section className="panel form-panel">
          <PanelHeader title={props.selectedTask ? "Task form" : "New task"} />
          <form className="form-stack" onSubmit={submitTask}>
            <label>
              Title
              <input value={taskForm.title} onChange={(event) => setTaskForm({ ...taskForm, title: event.target.value })} required minLength={3} />
            </label>
            <label>
              Description
              <textarea value={taskForm.description} onChange={(event) => setTaskForm({ ...taskForm, description: event.target.value })} required minLength={3} />
            </label>
            {!props.selectedTask && (
              <label>
                Project
                <select value={taskForm.projectId} onChange={(event) => setTaskForm({ ...taskForm, projectId: event.target.value })} required>
                  <option value="">Select project</option>
                  {props.projects.map((project) => (
                    <option key={project.id} value={project.id}>{project.name}</option>
                  ))}
                </select>
              </label>
            )}
            {!props.selectedTask && (
              <label>
                Assigned user id
                <input type="number" value={taskForm.assignedUserId} onChange={(event) => setTaskForm({ ...taskForm, assignedUserId: event.target.value })} required />
              </label>
            )}
            <div className="form-grid">
              <label>
                Priority
                <select value={taskForm.priority} onChange={(event) => setTaskForm({ ...taskForm, priority: Number(event.target.value) as TaskPriority })}>
                  {taskPriorityOptions.map((priority) => (
                    <option key={priority} value={priority}>{taskPriorityLabels[priority]}</option>
                  ))}
                </select>
              </label>
              <label>
                Due date
                <input type="date" value={taskForm.dueDate} onChange={(event) => setTaskForm({ ...taskForm, dueDate: event.target.value })} />
              </label>
            </div>
            <div className="button-row">
              <button className="primary-button" disabled={saving}>
                {saving ? <Loader2 className="spin" size={17} /> : <Save size={17} />}
                {props.selectedTask ? "Save task" : "Create task"}
              </button>
              <button type="button" className="ghost-button compact" onClick={() => {
                props.setSelectedTask(null);
                setTaskForm({ title: "", description: "", priority: 2, dueDate: "", projectId: "", assignedUserId: "" });
              }}>
                Clear
              </button>
            </div>
          </form>
        </section>
      )}
    </div>
  );
}

function CommentsPanel({
  user,
  task,
  comments,
  loading,
  onReload,
  onSuccess
}: {
  user: AuthUser;
  task: TaskItem;
  comments: CommentItem[];
  loading: boolean;
  onReload: () => void;
  onSuccess: (toast: Toast) => void;
}) {
  const [text, setText] = useState("");
  const submit = async (event: FormEvent) => {
    event.preventDefault();
    if (!text.trim()) return;
    try {
      await api.addComment(user.token, task.id, text.trim());
      setText("");
      onReload();
      onSuccess({ kind: "success", text: "Comment added." });
    } catch (error) {
      onSuccess({ kind: "error", text: messageFromError(error) });
    }
  };

  const remove = async (comment: CommentItem) => {
    try {
      await api.deleteComment(user.token, comment.id);
      onReload();
      onSuccess({ kind: "success", text: "Comment deleted." });
    } catch (error) {
      onSuccess({ kind: "error", text: messageFromError(error) });
    }
  };

  return (
    <div className="comments">
      <div className="section-title">
        <MessageSquare size={17} /> Comments
      </div>
      <form className="comment-form" onSubmit={submit}>
        <input value={text} onChange={(event) => setText(event.target.value)} placeholder="Add a comment" maxLength={100} />
        <button className="icon-button" aria-label="Add comment">
          <Send size={17} />
        </button>
      </form>

      {loading ? <LoadingState /> : comments.length === 0 ? <EmptyState text="No comments yet." /> : (
        <div className="comment-list">
          {comments.map((comment) => {
            const canDelete = user.role === 1 || user.id === comment.userId;
            return (
              <article className="comment" key={comment.id}>
                <div>
                  <strong>{comment.userFullName || `User ${comment.userId}`}</strong>
                  <small>{formatDate(comment.createdAt)}</small>
                  <p>{comment.text}</p>
                </div>
                {canDelete && (
                  <button className="icon-button danger" aria-label="Delete comment" onClick={() => remove(comment)}>
                    <Trash2 size={16} />
                  </button>
                )}
              </article>
            );
          })}
        </div>
      )}
    </div>
  );
}

function TaskList({ tasks, onSelect, selectedTask }: { tasks: TaskItem[]; onSelect: (task: TaskItem) => void; selectedTask?: TaskItem | null }) {
  return (
    <div className="entity-list">
      {tasks.map((task) => (
        <button className={`entity-button task ${selectedTask?.id === task.id ? "selected" : ""}`} key={task.id} onClick={() => onSelect(task)}>
          <span>
            <strong>{task.title}</strong>
            <small>{task.projectName || "Project"} • {formatDate(task.dueDate)}</small>
          </span>
          <span className="badge-stack">
            <StatusBadge tone={task.status === 4 ? "green" : task.status === 5 ? "gray" : "blue"}>{taskStatusLabels[task.status]}</StatusBadge>
            <StatusBadge tone={task.priority >= 3 ? "red" : "amber"}>{taskPriorityLabels[task.priority]}</StatusBadge>
          </span>
        </button>
      ))}
    </div>
  );
}

function Stat({ label, value }: { label: string; value: number }) {
  return (
    <article className="stat-card">
      <span>{label}</span>
      <strong>{value}</strong>
    </article>
  );
}

function Meta({ label, value }: { label: string; value: string }) {
  return (
    <div className="meta">
      <span>{label}</span>
      <strong>{value}</strong>
    </div>
  );
}

function PanelHeader({
  title,
  actionLabel,
  onAction,
  icon,
  onIconAction
}: {
  title: string;
  actionLabel?: string;
  onAction?: () => void;
  icon?: React.ReactNode;
  onIconAction?: () => void | Promise<void>;
}) {
  return (
    <div className="panel-header">
      <h2>{title}</h2>
      {actionLabel && onAction && <button className="text-button" onClick={onAction}>{actionLabel}</button>}
      {icon && onIconAction && <button className="icon-button" aria-label="Refresh" onClick={onIconAction}>{icon}</button>}
    </div>
  );
}

function StatusBadge({ children, tone }: { children: React.ReactNode; tone: "green" | "blue" | "amber" | "red" | "gray" }) {
  return <span className={`status-badge ${tone}`}>{children}</span>;
}

function Pagination<T>({ paged, onPage }: { paged: PagedResponse<T>; onPage: (page: number) => void | Promise<void> }) {
  if (paged.totalPages <= 1) return null;
  return (
    <div className="pagination">
      <button className="secondary-button compact" disabled={paged.pageNumber <= 1} onClick={() => onPage(paged.pageNumber - 1)}>Prev</button>
      <span>Page {paged.pageNumber} of {paged.totalPages}</span>
      <button className="secondary-button compact" disabled={paged.pageNumber >= paged.totalPages} onClick={() => onPage(paged.pageNumber + 1)}>Next</button>
    </div>
  );
}

function LoadingState() {
  return (
    <div className="state-row">
      <Loader2 className="spin" size={18} /> Loading
    </div>
  );
}

function EmptyState({ text }: { text: string }) {
  return <div className="empty-state">{text}</div>;
}
