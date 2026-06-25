export type UserRole = 1 | 2 | 3;
export type ProjectStatus = 1 | 2 | 3;
export type TaskStatus = 1 | 2 | 3 | 4 | 5;
export type TaskPriority = 1 | 2 | 3 | 4;

export type AuthUser = {
  id: number | null;
  fullName: string;
  email: string;
  role: UserRole;
  createdAt: string;
  token: string;
};

export type AuthResponseDto = {
  fullName: string;
  email: string;
  role: UserRole;
  createdAt: string;
  token: string;
};

export type PagedResponse<T> = {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
};

export type Project = {
  id: number;
  name: string;
  description: string;
  status: ProjectStatus;
  createdAt: string;
  updatedAt: string;
};

export type TaskItem = {
  id: number;
  title: string;
  projectName: string;
  description: string;
  assignedUserId: number | null;
  assignedUserFullName: string | null;
  status: TaskStatus;
  priority: TaskPriority;
  dueDate: string | null;
  createdAt: string;
  updatedAt: string;
};

export type CommentItem = {
  id: number;
  text: string;
  createdAt: string;
  taskItemId: number;
  taskTitle: string;
  userId: number;
  userFullName: string;
};

export const roleLabels: Record<UserRole, string> = {
  1: "Admin",
  2: "Manager",
  3: "Developer"
};

export const projectStatusLabels: Record<ProjectStatus, string> = {
  1: "Active",
  2: "Completed",
  3: "Archived"
};

export const taskStatusLabels: Record<TaskStatus, string> = {
  1: "To do",
  2: "In progress",
  3: "In review",
  4: "Done",
  5: "Cancelled"
};

export const taskPriorityLabels: Record<TaskPriority, string> = {
  1: "Low",
  2: "Medium",
  3: "High",
  4: "Critical"
};

export const taskStatusOptions: TaskStatus[] = [1, 2, 3, 4, 5];
export const taskPriorityOptions: TaskPriority[] = [1, 2, 3, 4];
