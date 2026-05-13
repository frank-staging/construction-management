<!-- src/views/dashboard/admin/ProjectsList.vue -->
<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
      <div>
        <h1 class="text-2xl md:text-3xl font-semibold text-slate-900 dark:text-white">
          All Projects
        </h1>
        <p class="text-slate-600 dark:text-slate-400 mt-1">Manage and monitor all projects</p>
      </div>
      <RouterLink
        to="/dashboard"
        class="inline-flex items-center gap-1 px-3 py-1.5 text-sm text-slate-600 hover:text-emerald-600 transition-colors"
      >
        <ArrowLeftIcon class="w-4 h-4" /> Back to Dashboard
      </RouterLink>
    </div>

    <!-- Filters Section -->
    <div
      class="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 p-4"
    >
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <!-- Search -->
        <div>
          <label class="block text-sm text-slate-700 dark:text-slate-300 mb-1">Search</label>
          <input
            v-model="filters.searchTerm"
            type="text"
            placeholder="Name, contractor, tender..."
            @input="debouncedSearch"
            class="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-emerald-500 dark:bg-slate-700 dark:text-white"
          />
        </div>

        <!-- County Filter -->
        <div>
          <label class="block text-sm text-slate-700 dark:text-slate-300 mb-1">County</label>
          <select
            v-model="filters.countyId"
            @change="applyFilters"
            class="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-emerald-500 dark:bg-slate-700 dark:text-white"
          >
            <option value="">All Counties</option>
            <option v-for="county in counties" :key="county.id" :value="county.id">
              {{ county.name }}
            </option>
          </select>
        </div>

        <!-- Progress Status Filter -->
        <div>
          <label class="block text-sm text-slate-700 dark:text-slate-300 mb-1"
            >Progress Status</label
          >
          <select
            v-model="filters.progressStatus"
            @change="applyFilters"
            class="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-emerald-500 dark:bg-slate-700 dark:text-white"
          >
            <option value="">All Statuses</option>
            <option value="OnTime">On Track</option>
            <option value="Slow">Slow</option>
            <option value="Delayed">Delayed</option>
            <option value="Ahead">Ahead</option>
          </select>
        </div>

        <!-- Assigned To Filter -->
        <div>
          <label class="block text-sm text-slate-700 dark:text-slate-300 mb-1">Assigned To</label>
          <select
            v-model="filters.assignedTo"
            @change="applyFilters"
            class="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-emerald-500 dark:bg-slate-700 dark:text-white"
          >
            <option value="">All</option>
            <option value="COW">Clerk of Works (COW)</option>
            <option value="TL">Technical Lead (TL)</option>
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
          v-if="filters.countyId"
          @click="clearFilter('countyId')"
          class="inline-flex items-center gap-1 px-2 py-1 bg-slate-100 dark:bg-slate-700 rounded-full text-xs"
        >
          County: {{ getCountyName(filters.countyId) }}
          <XIcon class="w-3 h-3" />
        </button>
        <button
          v-if="filters.progressStatus"
          @click="clearFilter('progressStatus')"
          class="inline-flex items-center gap-1 px-2 py-1 bg-slate-100 dark:bg-slate-700 rounded-full text-xs"
        >
          Status: {{ filters.progressStatus }}
          <XIcon class="w-3 h-3" />
        </button>
        <button
          v-if="filters.assignedTo"
          @click="clearFilter('assignedTo')"
          class="inline-flex items-center gap-1 px-2 py-1 bg-slate-100 dark:bg-slate-700 rounded-full text-xs"
        >
          Role: {{ filters.assignedTo }}
          <XIcon class="w-3 h-3" />
        </button>
        <button @click="clearAllFilters" class="text-xs text-emerald-600 hover:text-emerald-700">
          Clear all
        </button>
      </div>
    </div>

    <!-- DESKTOP VIEW: Table (hidden on mobile) -->
    <div
      class="hidden md:block bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 shadow-sm overflow-hidden"
    >
      <div class="overflow-x-auto">
        <table class="w-full text-sm">
          <thead class="bg-slate-50 dark:bg-slate-700/50 text-slate-600 dark:text-slate-300">
            <tr>
              <th
                class="px-6 py-3 text-left font-medium cursor-pointer hover:bg-slate-100 dark:hover:bg-slate-600"
                @click="sortBy('name')"
              >
                <div class="flex items-center gap-1">
                  Project Name
                  <SortIcon
                    v-if="sortField === 'name'"
                    :class="sortOrder === 'asc' ? 'rotate-180' : ''"
                    class="w-3 h-3"
                  />
                </div>
              </th>
              <th class="px-6 py-3 text-left font-medium">County</th>
              <th
                class="px-6 py-3 text-left font-medium cursor-pointer hover:bg-slate-100 dark:hover:bg-slate-600"
                @click="sortBy('currentPhysicalProgress')"
              >
                <div class="flex items-center gap-1">
                  Progress
                  <SortIcon
                    v-if="sortField === 'currentPhysicalProgress'"
                    :class="sortOrder === 'asc' ? 'rotate-180' : ''"
                    class="w-3 h-3"
                  />
                </div>
              </th>
              <th class="px-6 py-3 text-left font-medium">Status</th>
              <th class="px-6 py-3 text-left font-medium">COW</th>
              <th class="px-6 py-3 text-left font-medium">Consultant</th>
              <th
                class="px-6 py-3 text-left font-medium cursor-pointer hover:bg-slate-100 dark:hover:bg-slate-600"
                @click="sortBy('daysRemaining')"
              >
                <div class="flex items-center gap-1">
                  Days Left
                  <SortIcon
                    v-if="sortField === 'daysRemaining'"
                    :class="sortOrder === 'asc' ? 'rotate-180' : ''"
                    class="w-3 h-3"
                  />
                </div>
              </th>
              <th class="px-6 py-3 text-left font-medium">Last Update</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-200 dark:divide-slate-700">
            <tr
              v-for="project in sortedProjects"
              :key="project.id"
              @click="goToProject(project.id)"
              class="hover:bg-slate-50 dark:hover:bg-slate-700/50 cursor-pointer transition-colors"
            >
              <td class="px-6 py-4">
                <div>
                  <p class="font-medium text-slate-900 dark:text-white">{{ project.name }}</p>
                  <p class="text-xs text-slate-500 dark:text-slate-400 mt-1">
                    {{ project.tenderNumber }}
                  </p>
                </div>
              </td>
              <td class="px-6 py-4 text-slate-700 dark:text-slate-300">{{ project.county }}</td>
              <td class="px-6 py-4">
                <div class="flex items-center gap-2">
                  <span class="text-slate-900 dark:text-white font-medium"
                    >{{ project.currentPhysicalProgress }}%</span
                  >
                  <div class="w-20 bg-slate-200 dark:bg-slate-600 rounded-full h-1.5">
                    <div
                      class="h-1.5 rounded-full"
                      :class="getProgressBarClass(project)"
                      :style="{ width: `${project.currentPhysicalProgress}%` }"
                    ></div>
                  </div>
                </div>
              </td>
              <td class="px-6 py-4">
                <span
                  :class="[
                    'px-2 py-1 rounded-full text-xs font-medium',
                    getStatusBadgeClass(project.progressStatus),
                  ]"
                >
                  {{ project.progressStatus }}
                </span>
              </td>
              <td class="px-6 py-4 text-slate-700 dark:text-slate-300">
                {{ project.clerkOfWorks || "Unassigned" }}
              </td>
              <td class="px-6 py-4 text-slate-700 dark:text-slate-300">
                {{ project.consultantName || "N/A" }}
              </td>
              <td class="px-6 py-4">
                <span
                  :class="
                    project.daysRemaining < 30
                      ? 'text-amber-600 dark:text-amber-400 font-medium'
                      : 'text-slate-600 dark:text-slate-300'
                  "
                >
                  {{ project.daysRemaining }} days
                </span>
              </td>
              <td class="px-6 py-4 text-slate-500 dark:text-slate-400 text-xs">
                {{ formatDate(project.lastProgressUpdate) }}
              </td>
            </tr>
            <tr v-if="projects.length === 0 && !projectsLoading">
              <td colspan="8" class="px-6 py-12 text-center">
                <div class="flex flex-col items-center gap-3">
                  <FolderOpenIcon class="w-12 h-12 text-slate-300 dark:text-slate-600" />
                  <p class="text-slate-500 dark:text-slate-400">No projects found</p>
                  <button
                    @click="clearAllFilters"
                    class="text-sm text-emerald-600 hover:text-emerald-700"
                  >
                    Clear filters
                  </button>
                </div>
              </td>
            </tr>
            <tr v-if="projectsLoading">
              <td colspan="8" class="px-6 py-12 text-center">
                <Loader2Icon class="w-8 h-8 animate-spin text-emerald-600 mx-auto" />
                <p class="text-slate-500 dark:text-slate-400 mt-2">Loading projects...</p>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <div
        class="px-6 py-4 border-t border-slate-200 dark:border-slate-700 flex items-center justify-between flex-wrap gap-3"
      >
        <div class="text-sm text-slate-600 dark:text-slate-400">
          Showing {{ (pagination.pageNumber - 1) * pagination.pageSize + 1 }} -
          {{ Math.min(pagination.pageNumber * pagination.pageSize, pagination.totalRecords) }} of
          {{ pagination.totalRecords }} projects
        </div>
        <div class="flex items-center gap-2">
          <button
            @click="changePage(pagination.pageNumber - 1)"
            :disabled="!pagination.hasPreviousPage"
            class="px-3 py-1 border border-slate-300 dark:border-slate-600 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed hover:bg-slate-50 dark:hover:bg-slate-700 transition-colors"
          >
            Previous
          </button>
          <span class="text-sm text-slate-600 dark:text-slate-400">
            Page {{ pagination.pageNumber }} of {{ pagination.totalPages }}
          </span>
          <button
            @click="changePage(pagination.pageNumber + 1)"
            :disabled="!pagination.hasNextPage"
            class="px-3 py-1 border border-slate-300 dark:border-slate-600 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed hover:bg-slate-50 dark:hover:bg-slate-700 transition-colors"
          >
            Next
          </button>
        </div>
      </div>
    </div>

    <!-- MOBILE VIEW: Cards (visible only on mobile) -->
    <div class="md:hidden space-y-4">
      <!-- Loading State -->
      <div v-if="projectsLoading" class="text-center py-12">
        <Loader2Icon class="w-8 h-8 animate-spin text-emerald-600 mx-auto" />
        <p class="text-slate-500 dark:text-slate-400 mt-2">Loading projects...</p>
      </div>

      <!-- Empty State -->
      <div
        v-else-if="projects.length === 0"
        class="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 p-12 text-center"
      >
        <FolderOpenIcon class="w-12 h-12 text-slate-300 dark:text-slate-600 mx-auto mb-3" />
        <p class="text-slate-500 dark:text-slate-400">No projects found</p>
        <button
          @click="clearAllFilters"
          class="mt-3 text-sm text-emerald-600 hover:text-emerald-700"
        >
          Clear filters
        </button>
      </div>

      <!-- Project Cards -->
      <div
        v-for="project in sortedProjects"
        :key="project.id"
        @click="goToProject(project.id)"
        class="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 p-4 cursor-pointer hover:shadow-md transition-shadow"
      >
        <!-- Header: Project Name & Tender Number -->
        <div class="flex justify-between items-start mb-3">
          <div class="flex-1">
            <h3 class="font-semibold text-slate-900 dark:text-white text-base">
              {{ project.name }}
            </h3>
            <p class="text-xs text-slate-500 dark:text-slate-400 mt-0.5">
              {{ project.tenderNumber }}
            </p>
          </div>
          <span
            :class="[
              'px-2 py-1 rounded-full text-xs font-medium ml-2 whitespace-nowrap',
              getStatusBadgeClass(project.progressStatus),
            ]"
          >
            {{ project.progressStatus }}
          </span>
        </div>

        <!-- Project Details Grid -->
        <div class="grid grid-cols-2 gap-3 text-sm">
          <!-- County -->
          <div>
            <p class="text-xs text-slate-500 dark:text-slate-400">County</p>
            <p class="text-slate-700 dark:text-slate-300 font-medium">{{ project.county }}</p>
          </div>

          <!-- Progress -->
          <div>
            <p class="text-xs text-slate-500 dark:text-slate-400">Progress</p>
            <div class="flex items-center gap-2 mt-1">
              <span class="text-slate-900 dark:text-white font-medium"
                >{{ project.currentPhysicalProgress }}%</span
              >
              <div class="flex-1 bg-slate-200 dark:bg-slate-600 rounded-full h-1.5">
                <div
                  class="h-1.5 rounded-full"
                  :class="getProgressBarClass(project)"
                  :style="{ width: `${project.currentPhysicalProgress}%` }"
                ></div>
              </div>
            </div>
          </div>

          <!-- Clerk of Works -->
          <div>
            <p class="text-xs text-slate-500 dark:text-slate-400">COW</p>
            <p class="text-slate-700 dark:text-slate-300">
              {{ project.clerkOfWorks || "Unassigned" }}
            </p>
          </div>

          <!-- Consultant -->
          <div>
            <p class="text-xs text-slate-500 dark:text-slate-400">Consultant</p>
            <p class="text-slate-700 dark:text-slate-300">{{ project.consultantName || "N/A" }}</p>
          </div>

          <!-- Days Remaining -->
          <div>
            <p class="text-xs text-slate-500 dark:text-slate-400">Days Left</p>
            <p
              :class="
                project.daysRemaining < 30
                  ? 'text-amber-600 dark:text-amber-400 font-medium'
                  : 'text-slate-700 dark:text-slate-300'
              "
            >
              {{ project.daysRemaining }} days
            </p>
          </div>

          <!-- Last Update -->
          <div>
            <p class="text-xs text-slate-500 dark:text-slate-400">Last Update</p>
            <p class="text-slate-700 dark:text-slate-300 text-sm">
              {{ formatDate(project.lastProgressUpdate) }}
            </p>
          </div>
        </div>
      </div>

      <!-- Mobile Pagination -->
      <div v-if="projects.length > 0" class="flex items-center justify-between gap-3 pt-4">
        <div class="text-xs text-slate-600 dark:text-slate-400">
          {{ (pagination.pageNumber - 1) * pagination.pageSize + 1 }} -
          {{ Math.min(pagination.pageNumber * pagination.pageSize, pagination.totalRecords) }} of
          {{ pagination.totalRecords }}
        </div>
        <div class="flex items-center gap-2">
          <button
            @click="changePage(pagination.pageNumber - 1)"
            :disabled="!pagination.hasPreviousPage"
            class="px-3 py-1 text-sm border border-slate-300 dark:border-slate-600 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed hover:bg-slate-50 dark:hover:bg-slate-700 transition-colors"
          >
            Prev
          </button>
          <button
            @click="changePage(pagination.pageNumber + 1)"
            :disabled="!pagination.hasNextPage"
            class="px-3 py-1 text-sm border border-slate-300 dark:border-slate-600 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed hover:bg-slate-50 dark:hover:bg-slate-700 transition-colors"
          >
            Next
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from "vue";
import { useRouter, RouterLink } from "vue-router";
import { useProjectStore } from "@/stores/projectStore";
import { getStatusBadgeClass, getProgressBarClass, formatDate } from "@/utils/permissions";
import {
  ArrowLeft as ArrowLeftIcon,
  X as XIcon,
  SortAsc as SortIcon,
  FolderOpen as FolderOpenIcon,
  Loader2 as Loader2Icon,
} from "lucide-vue-next";

const router = useRouter();
const projectStore = useProjectStore();

// State
const filters = ref({
  searchTerm: "",
  countyId: "",
  progressStatus: "",
  assignedTo: "",
});
const sortField = ref("name");
const sortOrder = ref("asc");
const searchTimeout = ref(null);
const projectsLoading = ref(false);

// Computed - Direct from store
const projects = computed(() => projectStore.projects);
const pagination = computed(() => projectStore.pagination);

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

// Sorting - Client-side sorting
const sortedProjects = computed(() => {
  const sorted = [...projects.value];
  sorted.sort((a, b) => {
    let aVal = a[sortField.value];
    let bVal = b[sortField.value];
    if (sortField.value === "name") {
      aVal = a.name?.toLowerCase() || "";
      bVal = b.name?.toLowerCase() || "";
    }
    return sortOrder.value === "asc" ? (aVal > bVal ? 1 : -1) : aVal < bVal ? 1 : -1;
  });
  return sorted;
});

const hasActiveFilters = computed(() => {
  return (
    filters.value.searchTerm ||
    filters.value.countyId ||
    filters.value.progressStatus ||
    filters.value.assignedTo
  );
});

const getCountyName = (id) => {
  const county = counties.value.find((c) => c.id === id);
  return county ? county.name : id;
};

const goToProject = (projectId) => {
  router.push(`/dashboard/projects/${projectId}`);
};

const applyFilters = () => {
  const params = {
    pageNumber: 1,
    pageSize: pagination.value.pageSize,
    searchTerm: filters.value.searchTerm || undefined,
    progressStatus: filters.value.progressStatus || undefined,
  };

  if (filters.value.countyId) {
    params.countyId = filters.value.countyId;
  }

  if (filters.value.assignedTo) {
    params.role = filters.value.assignedTo;
  }

  projectStore.fetchProjects(params);
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
    countyId: "",
    progressStatus: "",
    assignedTo: "",
  };
  applyFilters();
};

const sortBy = (field) => {
  if (sortField.value === field) {
    sortOrder.value = sortOrder.value === "asc" ? "desc" : "asc";
  } else {
    sortField.value = field;
    sortOrder.value = "asc";
  }
};

const changePage = (page) => {
  if (page >= 1 && page <= pagination.value.totalPages) {
    const params = {
      pageNumber: page,
      pageSize: pagination.value.pageSize,
      searchTerm: filters.value.searchTerm || undefined,
      progressStatus: filters.value.progressStatus || undefined,
    };
    if (filters.value.countyId) params.countyId = filters.value.countyId;
    if (filters.value.assignedTo) params.role = filters.value.assignedTo;
    projectStore.fetchProjects(params);
  }
};

// Load initial data
onMounted(async () => {
  projectsLoading.value = true;
  try {
    await projectStore.fetchProjects({ pageSize: 10 });
  } finally {
    projectsLoading.value = false;
  }
});
</script>
