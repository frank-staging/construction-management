<!-- src/views/dashboard/admin/UsersList.vue -->
<template>
  <div class="space-y-4 md:space-y-6">
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
      <div>
        <h1 class="text-xl md:text-2xl lg:text-3xl font-semibold text-slate-900 dark:text-white">
          All Users
        </h1>
        <p class="text-sm text-slate-600 dark:text-slate-400 mt-1">
          Manage system users and their assignments
        </p>
      </div>
     <div class="flex items-center gap-2">
        <!-- Only show for SuperAdmin -->
        <RouterLink
          v-if="userRole === 'SuperAdmin'"
          to="/dashboard/users/register"
          class="inline-flex items-center gap-2 px-4 py-2 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg transition-colors text-sm"
        >
          <UserPlusIcon class="w-4 h-4" />
          Register New User
        </RouterLink>
        <RouterLink
          to="/dashboard"
          class="inline-flex items-center gap-1 px-3 py-1.5 text-sm text-slate-600 hover:text-emerald-600 transition-colors"
        >
          <ArrowLeftIcon class="w-4 h-4" /> Back to Dashboard
        </RouterLink>
      </div>
    </div>

    <!-- Filters -->
    <div
      class="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 p-4"
    >
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3 md:gap-4">
        <!-- Search -->
        <div>
          <label class="block text-sm text-slate-700 dark:text-slate-300 mb-1">Search</label>
          <input
            v-model="filters.searchTerm"
            type="text"
            placeholder="Name, email, phone..."
            @input="debouncedSearch"
            class="w-full px-3 py-2 text-sm border border-slate-300 dark:border-slate-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-emerald-500 dark:bg-slate-700 dark:text-white"
          />
        </div>

        <!-- Role Filter -->
        <div>
          <label class="block text-sm text-slate-700 dark:text-slate-300 mb-1">Role</label>
          <select
            v-model="filters.role"
            @change="applyFilters"
            class="w-full px-3 py-2 text-sm border border-slate-300 dark:border-slate-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-emerald-500 dark:bg-slate-700 dark:text-white"
          >
            <option value="">All Roles</option>
            <option value="COW">Clerk of Works (COW)</option>
            <option value="TL">Technical Lead (TL)</option>
            <option value="CS">Cluster Supervisor (CS)</option>
            <option value="RL">Regional Lead (RL)</option>
            <option value="CDH">County Director (CDH)</option>
          </select>
        </div>

        <!-- Status Filter -->
        <div>
          <label class="block text-sm text-slate-700 dark:text-slate-300 mb-1">Status</label>
          <select
            v-model="filters.isActive"
            @change="applyFilters"
            class="w-full px-3 py-2 text-sm border border-slate-300 dark:border-slate-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-emerald-500 dark:bg-slate-700 dark:text-white"
          >
            <option value="">All</option>
            <option value="true">Active</option>
            <option value="false">Inactive</option>
          </select>
        </div>

        <!-- County Filter -->
        <div>
          <label class="block text-sm text-slate-700 dark:text-slate-300 mb-1">County</label>
          <select
            v-model="filters.countyId"
            @change="applyFilters"
            class="w-full px-3 py-2 text-sm border border-slate-300 dark:border-slate-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-emerald-500 dark:bg-slate-700 dark:text-white"
          >
            <option value="">All Counties</option>
            <option v-for="county in counties" :key="county.id" :value="county.id">
              {{ county.name }}
            </option>
          </select>
        </div>
      </div>

      <!-- Active Filters Display -->
      <div
        v-if="hasActiveFilters"
        class="flex flex-wrap gap-2 mt-4 pt-4 border-t border-slate-200 dark:border-slate-700"
      >
        <span class="text-xs text-slate-500 dark:text-slate-400">Active filters:</span>
        <button
          v-if="filters.searchTerm"
          @click="clearFilter('searchTerm')"
          class="inline-flex items-center gap-1 px-2 py-1 bg-slate-100 dark:bg-slate-700 rounded-full text-xs"
        >
          Search: {{ filters.searchTerm }}
          <XIcon class="w-3 h-3" />
        </button>
        <button
          v-if="filters.role"
          @click="clearFilter('role')"
          class="inline-flex items-center gap-1 px-2 py-1 bg-slate-100 dark:bg-slate-700 rounded-full text-xs"
        >
          Role: {{ getRoleDisplayName(filters.role) }}
          <XIcon class="w-3 h-3" />
        </button>
        <button
          v-if="filters.isActive"
          @click="clearFilter('isActive')"
          class="inline-flex items-center gap-1 px-2 py-1 bg-slate-100 dark:bg-slate-700 rounded-full text-xs"
        >
          Status: {{ filters.isActive === "true" ? "Active" : "Inactive" }}
          <XIcon class="w-3 h-3" />
        </button>
        <button
          v-if="filters.countyId"
          @click="clearFilter('countyId')"
          class="inline-flex items-center gap-1 px-2 py-1 bg-slate-100 dark:bg-slate-700 rounded-full text-xs"
        >
          County: {{ getCountyName(filters.countyId) }}
          <XIcon class="w-3 h-3" />
        </button>
        <button @click="clearAllFilters" class="text-xs text-emerald-600 hover:text-emerald-700">
          Clear all
        </button>
      </div>
    </div>

    <!-- Mobile Card View (shows on mobile, hidden on desktop) -->
    <div class="block md:hidden space-y-3">
      <div
        v-for="user in users"
        :key="user.id"
        @click="goToUser(user.id)"
        class="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 p-4 cursor-pointer hover:shadow-md transition-shadow"
      >
        <div class="flex justify-between items-start mb-3">
          <div class="flex-1">
            <div class="flex items-center gap-2 flex-wrap">
              <h3 class="font-semibold text-slate-900 dark:text-white text-base">
                {{ user.fullName }}
              </h3>
              <span
                :class="[
                  'px-2 py-1 rounded-full text-xs font-medium',
                  getRoleBadgeClass(user.role),
                ]"
              >
                {{ getRoleDisplayName(user.role) }}
              </span>
            </div>
            <p class="text-xs text-slate-500 mt-1">ID: {{ user.id?.slice(0, 8) || "" }}...</p>
          </div>
          <span
            :class="user.isActive ? 'bg-emerald-100 text-emerald-700' : 'bg-red-100 text-red-700'"
            class="px-2 py-1 rounded-full text-xs font-medium"
          >
            {{ user.isActive ? "Active" : "Inactive" }}
          </span>
        </div>

        <div class="space-y-2 text-sm">
          <div class="flex justify-between">
            <span class="text-slate-500">Email:</span>
            <span class="text-slate-700 dark:text-slate-300 break-all text-right ml-2">{{
              user.email
            }}</span>
          </div>
          <div class="flex justify-between">
            <span class="text-slate-500">Phone:</span>
            <span class="text-slate-700 dark:text-slate-300">{{ user.phoneNumber || "N/A" }}</span>
          </div>
          <div class="flex justify-between">
            <span class="text-slate-500">Projects Assigned:</span>
            <span class="text-slate-700 dark:text-slate-300 font-medium">
              {{ user.projectAssignments?.length || 0 }}
            </span>
          </div>
          <div class="flex justify-between">
            <span class="text-slate-500">Location:</span>
            <span class="text-slate-700 dark:text-slate-300">
              {{ user.baseLocation?.county || getCountyFromBaseLocation(user) || "Not set" }}
            </span>
          </div>
          <div class="flex justify-between text-xs">
            <span class="text-slate-500">Last Login:</span>
            <span class="text-slate-500">{{ formatDate(user.lastLoginAt) }}</span>
          </div>
        </div>
      </div>
    </div>

    <!-- Desktop Table View (hidden on mobile) -->
    <div
      class="hidden md:block bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 shadow-sm overflow-hidden"
    >
      <div class="overflow-x-auto">
        <table class="w-full text-sm">
          <thead class="bg-slate-50 dark:bg-slate-700/50 text-slate-600 dark:text-slate-300">
            <tr>
              <th class="px-6 py-3 text-left font-medium">Name</th>
              <th class="px-6 py-3 text-left font-medium">Email</th>
              <th class="px-6 py-3 text-left font-medium">Phone</th>
              <th class="px-6 py-3 text-left font-medium">Role</th>
              <th class="px-6 py-3 text-left font-medium">Status</th>
              <th class="px-6 py-3 text-left font-medium">Projects</th>
              <th class="px-6 py-3 text-left font-medium">County</th>
              <th class="px-6 py-3 text-left font-medium">Last Login</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-200 dark:divide-slate-700">
            <tr
              v-for="user in users"
              :key="user.id"
              @click="goToUser(user.id)"
              class="hover:bg-slate-50 dark:hover:bg-slate-700/50 cursor-pointer transition-colors"
            >
              <td class="px-6 py-4">
                <div>
                  <p class="font-medium text-slate-900 dark:text-white">{{ user.fullName }}</p>
                  <p class="text-xs text-slate-500 dark:text-slate-400 mt-1">
                    ID: {{ user.id?.slice(0, 8) || "" }}...
                  </p>
                </div>
              </td>
              <td class="px-6 py-4 text-slate-700 dark:text-slate-300 max-w-xs truncate">
                {{ user.email }}
              </td>
              <td class="px-6 py-4 text-slate-700 dark:text-slate-300">
                {{ user.phoneNumber || "N/A" }}
              </td>
              <td class="px-6 py-4">
                <span
                  :class="[
                    'px-2 py-1 rounded-full text-xs font-medium',
                    getRoleBadgeClass(user.role),
                  ]"
                >
                  {{ getRoleDisplayName(user.role) }}
                </span>
              </td>
              <td class="px-6 py-4">
                <span
                  :class="
                    user.isActive ? 'bg-emerald-100 text-emerald-700' : 'bg-red-100 text-red-700'
                  "
                  class="px-2 py-1 rounded-full text-xs"
                >
                  {{ user.isActive ? "Active" : "Inactive" }}
                </span>
              </td>
              <td class="px-6 py-4 text-slate-700 dark:text-slate-300">
                {{ user.projectAssignments?.length || 0 }} assigned
              </td>
              <td class="px-6 py-4 text-slate-700 dark:text-slate-300">
                {{ getCountyFromBaseLocation(user) || "N/A" }}
              </td>
              <td class="px-6 py-4 text-slate-500 dark:text-slate-400 text-xs">
                {{ formatDate(user.lastLoginAt) }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Empty State -->
    <div
      v-if="users.length === 0 && !loading"
      class="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 p-12 text-center"
    >
      <UsersIcon class="w-12 h-12 text-slate-300 dark:text-slate-600 mx-auto mb-3" />
      <p class="text-slate-500 dark:text-slate-400">No users found</p>
      <button
        v-if="hasActiveFilters"
        @click="clearAllFilters"
        class="text-sm text-emerald-600 hover:text-emerald-700 mt-2"
      >
        Clear filters
      </button>
    </div>

    <!-- Loading State -->
    <div
      v-if="loading"
      class="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 p-12 text-center"
    >
      <Loader2Icon class="w-8 h-8 animate-spin text-emerald-600 mx-auto" />
      <p class="text-slate-500 dark:text-slate-400 mt-2">Loading users...</p>
    </div>

    <!-- Pagination -->
    <div
      v-if="totalRecords > 0"
      class="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 px-4 py-3 flex flex-col sm:flex-row items-center justify-between gap-3"
    >
      <div class="text-xs sm:text-sm text-slate-600 dark:text-slate-400 order-2 sm:order-1">
        Showing {{ startRecord }} - {{ endRecord }} of {{ totalRecords }} users
      </div>
      <div class="flex items-center gap-2 order-1 sm:order-2">
        <button
          @click="changePage(currentPage - 1)"
          :disabled="!hasPreviousPage"
          class="px-3 py-1 text-sm border border-slate-300 dark:border-slate-600 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed hover:bg-slate-50 dark:hover:bg-slate-700 transition-colors"
        >
          Previous
        </button>
        <span class="text-sm text-slate-600 dark:text-slate-400">
          Page {{ currentPage }} of {{ totalPages }}
        </span>
        <button
          @click="changePage(currentPage + 1)"
          :disabled="!hasNextPage"
          class="px-3 py-1 text-sm border border-slate-300 dark:border-slate-600 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed hover:bg-slate-50 dark:hover:bg-slate-700 transition-colors"
        >
          Next
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from "vue";
import { useRouter, RouterLink } from "vue-router";
import { useUserStore } from "@/stores/userStore";
import { useAuthStore } from "@/stores/AuthStore";  // ✅ ADD THIS
import { getRoleDisplayName, formatDate } from "@/utils/permissions";
import {
  ArrowLeft as ArrowLeftIcon,
  X as XIcon,
  Users as UsersIcon,
  Loader2 as Loader2Icon,
  UserPlusIcon,  // ✅ ADD THIS
} from "lucide-vue-next";

const router = useRouter();
const userStore = useUserStore();
const authStore = useAuthStore();  // ✅ ADD THIS

// ✅ ADD THIS - Get user role from auth store
const userRole = computed(() => authStore.userRole);

// State
const filters = ref({
  searchTerm: "",
  role: "",
  isActive: "",
  countyId: "",
});
const searchTimeout = ref(null);

// Counties data
const counties = ref([
  { id: "20000000-0000-0000-0000-000000000001", name: "Trans-Nzoia" },
  { id: "20000000-0000-0000-0000-000000000002", name: "Uasin Gishu" },
  { id: "20000000-0000-0000-0000-000000000003", name: "Elgeyo Marakwet" },
  { id: "20000000-0000-0000-0000-000000000004", name: "Nandi" },
  { id: "20000000-0000-0000-0000-000000000005", name: "Kericho" },
  { id: "20000000-0000-0000-0000-000000000006", name: "Bomet" },
  { id: "20000000-0000-0000-0000-000000000007", name: "Nakuru" },
  { id: "20000000-0000-0000-0000-000000000008", name: "Narok" },
  { id: "20000000-0000-0000-0000-000000000009", name: "Baringo" },
  { id: "20000000-0000-0000-0000-000000000010", name: "Samburu" },
  { id: "20000000-0000-0000-0000-000000000011", name: "Turkana" },
  { id: "20000000-0000-0000-0000-000000000012", name: "West Pokot" },
]);

// Computed from store
const users = computed(() => userStore.users);
const pagination = computed(() => userStore.pagination);
const loading = computed(() => userStore.loading);

const currentPage = computed(() => pagination.value.pageNumber);
const totalPages = computed(() => pagination.value.totalPages);
const totalRecords = computed(() => pagination.value.totalRecords);
const hasNextPage = computed(() => pagination.value.hasNextPage);
const hasPreviousPage = computed(() => pagination.value.hasPreviousPage);
const pageSize = computed(() => pagination.value.pageSize);

const startRecord = computed(() => (currentPage.value - 1) * pageSize.value + 1);
const endRecord = computed(() => Math.min(currentPage.value * pageSize.value, totalRecords.value));

const hasActiveFilters = computed(() => {
  return (
    filters.value.searchTerm ||
    filters.value.role ||
    filters.value.isActive ||
    filters.value.countyId
  );
});

// Helper functions
const getCountyName = (id) => {
  const county = counties.value.find((c) => c.id === id);
  return county ? county.name : id;
};

const getCountyFromBaseLocation = (user) => {
  if (user.countyId) {
    return getCountyName(user.countyId);
  }
  if (user.baseLocation?.county) {
    return user.baseLocation.county;
  }
  return null;
};

const getRoleBadgeClass = (role) => {
  const classes = {
    CS: "bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-400",
    RL: "bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400",
    CDH: "bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400",
    COW: "bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400",
    TL: "bg-slate-100 text-slate-700 dark:bg-slate-700 dark:text-slate-300",
  };
  return classes[role] || "bg-slate-100 text-slate-700";
};

const goToUser = (id) => {
  router.push(`/dashboard/users/${id}`);
};

const applyFilters = () => {
  const params = {
    pageNumber: 1,
    pageSize: pageSize.value,
    searchTerm: filters.value.searchTerm || undefined,
    role: filters.value.role || undefined,
    isActive: filters.value.isActive || undefined,
    countyId: filters.value.countyId || undefined,
  };
  userStore.fetchUsers(params);
};

const debouncedSearch = () => {
  if (searchTimeout.value) clearTimeout(searchTimeout.value);
  searchTimeout.value = setTimeout(() => {
    applyFilters();
  }, 300);
};

const clearFilter = (filter) => {
  filters.value[filter] = "";
  applyFilters();
};

const clearAllFilters = () => {
  filters.value = {
    searchTerm: "",
    role: "",
    isActive: "",
    countyId: "",
  };
  applyFilters();
};

const changePage = (page) => {
  if (page >= 1 && page <= totalPages.value) {
    userStore.fetchUsers({
      pageNumber: page,
      pageSize: pageSize.value,
      searchTerm: filters.value.searchTerm || undefined,
      role: filters.value.role || undefined,
      isActive: filters.value.isActive || undefined,
      countyId: filters.value.countyId || undefined,
    });
  }
};

// Load initial data
onMounted(() => {
  userStore.fetchUsers({ pageSize: 10 });
});
</script>
