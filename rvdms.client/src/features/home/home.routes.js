// home.routes.js
import AppLayout from "@/layouts/AppLayout.vue";
import ClerkLayout from "@/layouts/ClerkLayout.vue";

export default [
  {
    name: "home",
    path: "/",
    component: () => import("./views/HomeView.vue"),
    meta: { requiresAuth: false },
  },

  // SUPER ADMIN ROUTES - Wrapped in AppLayout
  {
    path: "/dashboard",
    component: AppLayout,
    meta: { requiresAuth: true, role: "SuperAdmin" },
    children: [
      {
        name: "superAdminDashboard",
        path: "",
        component: () => import("./views/dashboard/superAdmin/SuperAdminDashboard.vue"),
      },
      {
        name: "superAdminProjects",
        path: "projects",
        component: () => import("./views/dashboard/admin/ProjectsList.vue"),
      },
      {
  path: "/dashboard/users/register",
  name: "registerUser",
  component: () => import("./views/dashboard/superAdmin/RegisterUser.vue"),
  meta: { requiresAuth: true, role: "SuperAdmin" },
},
      {
        name: "superAdminProjectDetails",
        path: "projects/:id",
        component: () => import("./views/dashboard/admin/ProjectDetails.vue"),
      },
      {
        name: "superAdminUsers",
        path: "users",
        component: () => import("./views/dashboard/admin/UsersList.vue"),
      },
      {
        name: "superAdminUserDetails",
        path: "users/:id",
        component: () => import("./views/dashboard/admin/UserDetails.vue"),
      },
      
    ],
  },

  // LEADERSHIP ROUTES (RL, CDH, TL, CS) - All wrapped in AppLayout
  {
    path: "/dashboard",
    component: AppLayout,
    meta: { requiresAuth: true, roles: ["RL", "CDH", "TL", "CS"] },
    children: [
      {
        name: "adminDashboard",
        path: "",
        component: () => import("./views/dashboard/admin/DashboardHome.vue"),
      },
      {
        name: "projectsList",
        path: "projects",
        component: () => import("./views/dashboard/admin/ProjectsList.vue"),
      },
      {
        name: "projectDetails",
        path: "projects/:id",
        component: () => import("./views/dashboard/admin/ProjectDetails.vue"),
      },
      {
        name: "usersList",
        path: "users",
        component: () => import("./views/dashboard/admin/UsersList.vue"),
      },
      {
        name: "userDetails",
        path: "users/:id",
        component: () => import("./views/dashboard/admin/UserDetails.vue"),
      },
    ],
  },

  // CLERK OF WORKS ROUTES - Wrapped in ClerkLayout
  {
    path: "/dashboard/clerk",
    component: ClerkLayout,
    meta: { requiresAuth: true, role: "COW" },
    children: [
      {
        name: "clerkDashboard",
        path: "",
        component: () => import("./views/dashboard/ClerkDashboard.vue"),
      },
    ],
  },

  // PROFILE PAGE - Standalone (no sidebar)
  {
    path: "/profile",
    name: "profile",
    component: () => import("./views/ProfileView.vue"),
    meta: {
      requiresAuth: true,
      title: "My Profile",
    },
  },
];