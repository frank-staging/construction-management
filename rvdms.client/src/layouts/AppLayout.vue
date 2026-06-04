<!-- src/layouts/AppLayout.vue -->
<template>
  <div class="min-h-screen bg-slate-50 dark:bg-slate-900">
    <!-- Top Header -->
    <header
      class="bg-white dark:bg-slate-800 border-b border-slate-200 dark:border-slate-700 sticky top-0 z-40"
    >
      <div class="flex items-center justify-between px-4 md:px-6 py-3">
        <div class="flex items-center gap-3">
          <!-- Mobile Menu Button -->
          <button
            @click="toggleMobileMenu"
            class="lg:hidden p-2 rounded-lg hover:bg-slate-100 dark:hover:bg-slate-700 transition-colors"
          >
            <MenuIcon class="w-5 h-5 text-slate-600 dark:text-slate-400" />
          </button>

          <img
            src="@/assets/logos/ministry.jpg"
            alt="Ministry Logo"
            class="h-8 w-8 md:h-10 md:w-10 object-contain"
          />
          <div class="hidden sm:block">
            <h1 class="text-sm font-semibold text-slate-900 dark:text-white">
              Affordable Housing Programme
            </h1>
            <p class="text-xs text-slate-600 dark:text-slate-400">{{ roleTitle }}</p>
          </div>
        </div>

        <div class="flex items-center gap-2 md:gap-3">
          <button
            @click="toggleTheme"
            class="p-2 rounded-lg hover:bg-slate-100 dark:hover:bg-slate-700 transition-colors"
          >
            <SunIcon v-if="themeStore.isDark" class="w-4 h-4 md:w-5 md:h-5 text-amber-400" />
            <MoonIcon v-else class="w-4 h-4 md:w-5 md:h-5 text-slate-600" />
          </button>

          <div class="relative" ref="profileMenuRef">
            <button
              @click="showProfileMenu = !showProfileMenu"
              class="flex items-center gap-2 px-2 md:px-3 py-2 bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-lg hover:bg-slate-50 dark:hover:bg-slate-700 transition-colors"
            >
              <div
                class="w-7 h-7 md:w-8 md:h-8 bg-emerald-100 dark:bg-emerald-900/30 rounded-full flex items-center justify-center"
              >
                <span class="text-xs md:text-sm font-medium text-emerald-700 dark:text-emerald-400">{{
                  userInitials
                }}</span>
              </div>
              <div class="hidden sm:block text-left">
                <p class="text-xs md:text-sm font-medium text-slate-900 dark:text-white">
                  {{ user?.fullName }}
                </p>
                <p class="text-xs text-slate-500">{{ getRoleDisplayName(userRole) }}</p>
              </div>
              <ChevronDownIcon class="w-3 h-3 md:w-4 md:h-4 text-slate-400" />
            </button>

            <div
              v-if="showProfileMenu"
              class="absolute right-0 mt-2 w-48 bg-white dark:bg-slate-800 rounded-lg shadow-lg border border-slate-200 dark:border-slate-700 py-1 z-50"
            >
              <RouterLink
                to="/profile"
                class="flex items-center gap-2 px-4 py-2 text-sm text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700"
                @click="showProfileMenu = false"
              >
                <UserIcon class="w-4 h-4" /> My Profile
              </RouterLink>
              <button
                @click="logout"
                class="flex items-center gap-2 px-4 py-2 text-sm text-red-600 hover:bg-red-50 dark:hover:bg-red-900/30 w-full text-left"
              >
                <LogOutIcon class="w-4 h-4" /> Logout
              </button>
            </div>
          </div>
        </div>
      </div>
    </header>

    <div class="flex relative">
      <!-- Mobile Sidebar Overlay -->
      <div
        v-if="mobileMenuOpen"
        class="fixed inset-0 bg-black/50 z-40 lg:hidden"
        @click="toggleMobileMenu"
      ></div>

      <!-- Sidebar - Desktop: always visible, Mobile: slide from left -->
      <aside
        :class="[
          'fixed lg:sticky top-[57px] h-[calc(100vh-57px)] bg-white dark:bg-slate-800 border-r border-slate-200 dark:border-slate-700 transition-transform duration-300 z-50 w-64',
          mobileMenuOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0',
        ]"
      >
        <nav class="p-4 space-y-1 overflow-y-auto h-full">
          <!-- SUPER ADMIN MENU -->
          <template v-if="userRole === 'SuperAdmin'">
            <RouterLink
              to="/dashboard"
              class="flex items-center gap-3 px-4 py-2.5 rounded-lg transition-colors text-sm"
              :class="
                isActive('/dashboard')
                  ? 'bg-emerald-50 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-400 font-medium'
                  : 'text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700'
              "
              @click="mobileMenuOpen = false"
            >
              <LayoutDashboardIcon class="w-5 h-5" />
              <span>Dashboard</span>
            </RouterLink>

            <RouterLink
              to="/dashboard/projects"
              class="flex items-center gap-3 px-4 py-2.5 rounded-lg transition-colors text-sm"
              :class="
                isActive('/dashboard/projects')
                  ? 'bg-emerald-50 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-400 font-medium'
                  : 'text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700'
              "
              @click="mobileMenuOpen = false"
            >
              <FolderKanbanIcon class="w-5 h-5" />
              <span>All Projects</span>
              <span class="ml-auto text-xs text-slate-400">{{ projectCount }}</span>
            </RouterLink>

            <RouterLink
              to="/dashboard/users"
              class="flex items-center gap-3 px-4 py-2.5 rounded-lg transition-colors text-sm"
              :class="
                isActive('/dashboard/users')
                  ? 'bg-emerald-50 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-400 font-medium'
                  : 'text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700'
              "
              @click="mobileMenuOpen = false"
            >
              <UsersIcon class="w-5 h-5" />
              <span>All Users</span>
              <span class="ml-auto text-xs text-slate-400">{{ userCount }}</span>
            </RouterLink>
          </template>

          <!-- LEADERSHIP MENU (RL, CDH, TL, CS) -->
          <template v-else-if="['RL', 'CDH', 'TL', 'CS'].includes(userRole)">
            <RouterLink
              to="/dashboard"
              class="flex items-center gap-3 px-4 py-2.5 rounded-lg transition-colors text-sm"
              :class="
                isActive('/dashboard')
                  ? 'bg-emerald-50 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-400 font-medium'
                  : 'text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700'
              "
              @click="mobileMenuOpen = false"
            >
              <LayoutDashboardIcon class="w-5 h-5" />
              <span>Dashboard</span>
            </RouterLink>

            <RouterLink
              to="/dashboard/projects"
              class="flex items-center gap-3 px-4 py-2.5 rounded-lg transition-colors text-sm"
              :class="
                isActive('/dashboard/projects')
                  ? 'bg-emerald-50 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-400 font-medium'
                  : 'text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700'
              "
              @click="mobileMenuOpen = false"
            >
              <FolderKanbanIcon class="w-5 h-5" />
              <span>All Projects</span>
              <span class="ml-auto text-xs text-slate-400">{{ projectCount }}</span>
            </RouterLink>

            <RouterLink
              v-if="userRole !== 'CS' && userRole !== 'TL'"
              to="/dashboard/users"
              class="flex items-center gap-3 px-4 py-2.5 rounded-lg transition-colors text-sm"
              :class="
                isActive('/dashboard/users')
                  ? 'bg-emerald-50 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-400 font-medium'
                  : 'text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700'
              "
              @click="mobileMenuOpen = false"
            >
              <UsersIcon class="w-5 h-5" />
              <span>All Users</span>
              <span class="ml-auto text-xs text-slate-400">{{ userCount }}</span>
            </RouterLink>
          </template>

          <!-- CLERK OF WORKS MENU -->
          <template v-else-if="userRole === 'COW'">
            <RouterLink
              to="/dashboard/clerk"
              class="flex items-center gap-3 px-4 py-2.5 rounded-lg transition-colors text-sm"
              :class="
                isActive('/dashboard/clerk')
                  ? 'bg-emerald-50 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-400 font-medium'
                  : 'text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700'
              "
              @click="mobileMenuOpen = false"
            >
              <LayoutDashboardIcon class="w-5 h-5" />
              <span>My Dashboard</span>
            </RouterLink>

            <RouterLink
              v-if="userRole === 'COW'"
              to="/dashboard/clerk/projects"
              class="flex items-center gap-3 px-4 py-2.5 rounded-lg transition-colors text-sm"
              :class="
                isActive('/dashboard/clerk/projects')
                  ? 'bg-emerald-50 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-400 font-medium'
                  : 'text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700'
              "
              @click="mobileMenuOpen = false"
            >
              <FolderKanbanIcon class="w-5 h-5" />
              <span>My Projects</span>
            </RouterLink>
          </template>
        </nav>
      </aside>

      <main class="flex-1 w-full min-w-0 p-4 md:p-6 overflow-x-auto">
        <RouterView />
      </main>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from "vue";
import { useRouter, useRoute } from "vue-router";
import { useAuthStore } from "@/stores/AuthStore";
import { useProjectStore } from "@/stores/projectStore";
import { useUserStore } from "@/stores/userStore";
import { useThemeStore } from "@/stores/themeStore";
import { getRoleDisplayName } from "@/utils/permissions";
import {
  LayoutDashboard as LayoutDashboardIcon,
  FolderKanban as FolderKanbanIcon,
  Users as UsersIcon,
  ChevronDown as ChevronDownIcon,
  User as UserIcon,
  LogOut as LogOutIcon,
  Sun as SunIcon,
  Moon as MoonIcon,
  Menu as MenuIcon,
} from "lucide-vue-next";

const router = useRouter();
const route = useRoute();
const authStore = useAuthStore();
const projectStore = useProjectStore();
const userStore = useUserStore();
const themeStore = useThemeStore();

const showProfileMenu = ref(false);
const mobileMenuOpen = ref(false);
const profileMenuRef = ref(null);

const user = computed(() => authStore.user);
const userRole = computed(() => authStore.userRole);

const userInitials = computed(() => {
  if (!user.value) return "U";
  const first = user.value.firstName?.charAt(0) || "";
  const last = user.value.lastName?.charAt(0) || "";
  return (first + last).toUpperCase() || "U";
});

const roleTitle = computed(() => {
  const titles = {
    SuperAdmin: "Super Administrator Dashboard",
    RL: "Regional Lead Dashboard",
    CS: "Cluster Supervisor Dashboard",
    CDH: "County Director Dashboard",
    TL: "Technical Lead Dashboard",
    COW: "Clerk of Works Dashboard",
  };
  return titles[userRole.value] || "Leadership Dashboard";
});

const projectCount = computed(() => projectStore.pagination?.totalRecords || 0);
const userCount = computed(() => userStore.pagination?.totalRecords || 0);

const isActive = (path) => {
  if (path === "/dashboard") return route.path === "/dashboard";
  if (path === "/dashboard/clerk") return route.path === "/dashboard/clerk";
  return route.path.startsWith(path);
};

const toggleTheme = () => themeStore.toggleTheme();
const toggleMobileMenu = () => {
  mobileMenuOpen.value = !mobileMenuOpen.value;
};

const logout = () => {
  authStore.logout();
  router.push("/login");
};

const handleClickOutside = (e) => {
  if (profileMenuRef.value && !profileMenuRef.value.contains(e.target)) {
    showProfileMenu.value = false;
  }
};

// Close mobile menu on window resize if screen becomes large
const handleResize = () => {
  if (window.innerWidth >= 1024 && mobileMenuOpen.value) {
    mobileMenuOpen.value = false;
  }
};

onMounted(async () => {
  // Only fetch counts if user is authenticated and has access
  if (authStore.isAuthenticated && userRole.value !== "COW") {
    await Promise.all([
      projectStore.fetchProjects({ pageSize: 1 }),
      userStore.fetchUsers({ pageSize: 1 }),
    ]);
  }
  themeStore.initTheme();
  document.addEventListener("click", handleClickOutside);
  window.addEventListener("resize", handleResize);
});

onUnmounted(() => {
  document.removeEventListener("click", handleClickOutside);
  window.removeEventListener("resize", handleResize);
});
</script>