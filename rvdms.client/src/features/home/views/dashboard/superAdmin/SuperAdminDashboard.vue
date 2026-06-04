<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
      <div>
        <h1 class="text-2xl md:text-3xl font-semibold text-slate-900 dark:text-white">
          Super Admin Dashboard
        </h1>
        <p class="text-slate-600 dark:text-slate-400 mt-1">
          System-wide overview and analytics
        </p>
      </div>
      <div class="flex items-center gap-2">
        <button
          @click="refreshDashboard"
          :disabled="loading"
          class="inline-flex items-center gap-2 px-3 py-2 text-sm bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-lg hover:bg-slate-50 dark:hover:bg-slate-700 transition-colors"
        >
          <RefreshCwIcon :class="loading ? 'animate-spin' : ''" class="w-4 h-4" />
          Refresh
        </button>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="flex justify-center items-center py-20">
      <Loader2Icon class="w-8 h-8 animate-spin text-emerald-600" />
      <span class="ml-2 text-slate-600 dark:text-slate-400">Loading dashboard data...</span>
    </div>

    <div v-else-if="dashboardData">
      <!-- Overview Stats Cards -->
      <div class="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <div
          class="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 p-4"
        >
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm text-slate-500 dark:text-slate-400">Total Projects</p>
              <p class="text-2xl font-bold text-slate-900 dark:text-white">
                {{ dashboardData.overview.totalProjects }}
              </p>
            </div>
            <div class="w-10 h-10 bg-emerald-100 dark:bg-emerald-900/30 rounded-lg flex items-center justify-center">
              <FolderKanbanIcon class="w-5 h-5 text-emerald-600" />
            </div>
          </div>
          <div class="mt-2 flex items-center gap-2 text-xs">
            <span class="text-green-600">{{ dashboardData.overview.activeProjects }} active</span>
            <span class="text-slate-300">|</span>
            <span class="text-amber-600">{{ dashboardData.overview.delayedProjects }} delayed</span>
          </div>
        </div>

        <div
          class="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 p-4"
        >
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm text-slate-500 dark:text-slate-400">Total Users</p>
              <p class="text-2xl font-bold text-slate-900 dark:text-white">
                {{ dashboardData.overview.totalUsers }}
              </p>
            </div>
            <div class="w-10 h-10 bg-blue-100 dark:bg-blue-900/30 rounded-lg flex items-center justify-center">
              <UsersIcon class="w-5 h-5 text-blue-600" />
            </div>
          </div>
          <div class="mt-2 text-xs text-slate-500">
            {{ dashboardData.overview.activeUsers }} active today
          </div>
        </div>

        <div
          class="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 p-4"
        >
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm text-slate-500 dark:text-slate-400">Overall Progress</p>
              <p class="text-2xl font-bold text-slate-900 dark:text-white">
                {{ dashboardData.overview.overallProgress.toFixed(1) }}%
              </p>
            </div>
            <div class="w-10 h-10 bg-amber-100 dark:bg-amber-900/30 rounded-lg flex items-center justify-center">
              <TrendingUpIcon class="w-5 h-5 text-amber-600" />
            </div>
          </div>
          <div class="mt-2">
            <div class="w-full bg-slate-200 dark:bg-slate-700 rounded-full h-1.5">
              <div
                class="bg-amber-500 h-1.5 rounded-full"
                :style="{ width: `${dashboardData.overview.overallProgress}%` }"
              ></div>
            </div>
          </div>
        </div>

        <div
          class="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 p-4"
        >
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm text-slate-500 dark:text-slate-400">Budget Utilization</p>
              <p class="text-2xl font-bold text-slate-900 dark:text-white">
                {{ dashboardData.performance.budgetAdherenceRate.toFixed(1) }}%
              </p>
            </div>
            <div class="w-10 h-10 bg-purple-100 dark:bg-purple-900/30 rounded-lg flex items-center justify-center">
              <DollarSignIcon class="w-5 h-5 text-purple-600" />
            </div>
          </div>
          <div class="mt-2 text-xs text-slate-500">
            {{ formatCurrency(dashboardData.projects.spentBudget) }} / {{ formatCurrency(dashboardData.projects.totalBudget) }}
          </div>
        </div>
      </div>

      <!-- Charts Row -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Monthly Progress Chart -->
        <div class="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 p-4">
          <h3 class="text-base font-semibold text-slate-900 dark:text-white mb-4">
            Monthly Progress Trend
          </h3>
          <div class="h-64">
            <canvas ref="monthlyChartRef"></canvas>
          </div>
        </div>

        <!-- User Distribution Chart -->
        <div class="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 p-4">
          <h3 class="text-base font-semibold text-slate-900 dark:text-white mb-4">
            User Distribution by Role
          </h3>
          <div class="h-64">
            <canvas ref="userChartRef"></canvas>
          </div>
        </div>
      </div>

      <!-- Regional Stats Table -->
      <div class="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 overflow-hidden">
        <div class="px-4 py-3 border-b border-slate-200 dark:border-slate-700">
          <h3 class="text-base font-semibold text-slate-900 dark:text-white">
            Regional Performance
          </h3>
        </div>
        <div class="overflow-x-auto">
          <table class="w-full text-sm">
            <thead class="bg-slate-50 dark:bg-slate-700/50">
              <tr>
                <th class="px-4 py-3 text-left font-medium">County</th>
                <th class="px-4 py-3 text-center font-medium">Projects</th>
                <th class="px-4 py-3 text-center font-medium">Avg Progress</th>
                <th class="px-4 py-3 text-center font-medium">Delayed</th>
                <th class="px-4 py-3 text-center font-medium">Status</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-200 dark:divide-slate-700">
              <tr v-for="region in dashboardData.regionalStats" :key="region.countyName">
                <td class="px-4 py-3 font-medium">{{ region.countyName }}</td>
                <td class="px-4 py-3 text-center">{{ region.projectCount }}</td>
                <td class="px-4 py-3 text-center">
                  <div class="flex items-center justify-center gap-2">
                    <span>{{ region.averageProgress.toFixed(1) }}%</span>
                    <div class="w-16 bg-slate-200 rounded-full h-1.5">
                      <div
                        class="h-1.5 rounded-full"
                        :class="getProgressBarColor(region.averageProgress)"
                        :style="{ width: `${region.averageProgress}%` }"
                      ></div>
                    </div>
                  </div>
                </td>
                <td class="px-4 py-3 text-center">
                  <span class="text-amber-600">{{ region.delayedProjects }}</span>
                </td>
                <td class="px-4 py-3 text-center">
                  <span
                    :class="[
                      'px-2 py-1 rounded-full text-xs font-medium',
                      getStatusBadgeForProgress(region.averageProgress, region.delayedProjects),
                    ]"
                  >
                    {{ getRegionStatus(region.averageProgress, region.delayedProjects) }}
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Recent Activity -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Recent Projects -->
        <div class="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700">
          <div class="px-4 py-3 border-b border-slate-200 dark:border-slate-700">
            <h3 class="text-base font-semibold text-slate-900 dark:text-white">
              Recently Added Projects
            </h3>
          </div>
          <div class="divide-y divide-slate-200 dark:divide-slate-700">
            <div
              v-for="project in dashboardData.recentProjects"
              :key="project.id"
              class="px-4 py-3 hover:bg-slate-50 dark:hover:bg-slate-700/50 cursor-pointer"
              @click="goToProject(project.id)"
            >
              <div class="flex justify-between items-start">
                <div>
                  <p class="font-medium text-slate-900 dark:text-white">{{ project.name }}</p>
                  <p class="text-xs text-slate-500">{{ project.county }}</p>
                </div>
                <div class="text-right">
                  <p class="text-sm font-medium">{{ project.progress }}%</p>
                  <p class="text-xs text-slate-500">{{ formatDate(project.createdAt) }}</p>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Recent Users -->
        <div class="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700">
          <div class="px-4 py-3 border-b border-slate-200 dark:border-slate-700">
            <h3 class="text-base font-semibold text-slate-900 dark:text-white">
              Recently Added Users
            </h3>
          </div>
          <div class="divide-y divide-slate-200 dark:divide-slate-700">
            <div
              v-for="user in dashboardData.recentUsers"
              :key="user.id"
              class="px-4 py-3"
            >
              <div class="flex justify-between items-start">
                <div>
                  <p class="font-medium text-slate-900 dark:text-white">{{ user.name }}</p>
                  <p class="text-xs text-slate-500">{{ user.email }}</p>
                </div>
                <div class="text-right">
                  <span class="px-2 py-1 bg-slate-100 dark:bg-slate-700 rounded-full text-xs">
                    {{ user.role }}
                  </span>
                  <p class="text-xs text-slate-500 mt-1">{{ formatDate(user.createdAt) }}</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { useRouter } from "vue-router";
import { useDashboardStore } from "@/stores/dashboardStore";
import {
  FolderKanban as FolderKanbanIcon,
  Users as UsersIcon,
  TrendingUp as TrendingUpIcon,
  DollarSign as DollarSignIcon,
  RefreshCw as RefreshCwIcon,
  Loader2 as Loader2Icon,
} from "lucide-vue-next";
import Chart from "chart.js/auto";

const router = useRouter();
const dashboardStore = useDashboardStore();

const loading = ref(false);
const dashboardData = ref(null);
const monthlyChartRef = ref(null);
const userChartRef = ref(null);
let monthlyChart = null;
let userChart = null;

const formatCurrency = (value) => {
  return new Intl.NumberFormat('en-KE', {
    style: 'currency',
    currency: 'KES',
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(value);
};

const formatDate = (dateString) => {
  if (!dateString) return 'N/A';
  const date = new Date(dateString);
  return date.toLocaleDateString('en-KE');
};

const getProgressBarColor = (progress) => {
  if (progress >= 75) return 'bg-green-500';
  if (progress >= 50) return 'bg-emerald-500';
  if (progress >= 25) return 'bg-amber-500';
  return 'bg-red-500';
};

const getStatusBadgeForProgress = (progress, delayed) => {
  if (delayed > 0) return 'bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400';
  if (progress >= 75) return 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400';
  if (progress >= 50) return 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400';
  if (progress >= 25) return 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400';
  return 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400';
};

const getRegionStatus = (progress, delayed) => {
  if (delayed > 0) return 'At Risk';
  if (progress >= 75) return 'Excellent';
  if (progress >= 50) return 'Good';
  if (progress >= 25) return 'Moderate';
  return 'Poor';
};

const goToProject = (projectId) => {
  router.push(`/dashboard/projects/${projectId}`);
};

const refreshDashboard = async () => {
  loading.value = true;
  try {
    dashboardData.value = await dashboardStore.fetchSuperAdminDashboard();
    initCharts();
  } finally {
    loading.value = false;
  }
};

const initCharts = () => {
  if (!dashboardData.value) return;

  // Monthly Progress Chart
  if (monthlyChartRef.value) {
    if (monthlyChart) monthlyChart.destroy();
    
    const monthlyData = dashboardData.value.performance.monthlyProgress;
    monthlyChart = new Chart(monthlyChartRef.value, {
      type: 'line',
      data: {
        labels: monthlyData.map(m => `${m.month} ${m.year}`),
        datasets: [
          {
            label: 'Average Progress (%)',
            data: monthlyData.map(m => m.progressPercentage),
            borderColor: '#10b981',
            backgroundColor: 'rgba(16, 185, 129, 0.1)',
            fill: true,
            tension: 0.4,
          },
          {
            label: 'Projects Updated',
            data: monthlyData.map(m => m.projectsUpdated),
            borderColor: '#f59e0b',
            backgroundColor: 'rgba(245, 158, 11, 0.1)',
            fill: true,
            tension: 0.4,
            yAxisID: 'y1',
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { position: 'top' },
        },
        scales: {
          y: { title: { display: true, text: 'Progress (%)' } },
          y1: {
            position: 'right',
            title: { display: true, text: 'Projects Updated' },
            grid: { drawOnChartArea: false },
          },
        },
      },
    });
  }

  // User Distribution Chart
  if (userChartRef.value) {
    if (userChart) userChart.destroy();
    
    const userStats = dashboardData.value.users;
    userChart = new Chart(userChartRef.value, {
      type: 'doughnut',
      data: {
        labels: ['Regional Leads', 'County Directors', 'Technical Leads', 'Cluster Supervisors', 'Clerks of Works'],
        datasets: [
          {
            data: [
              userStats.regionalLeads,
              userStats.countyDirectors,
              userStats.technicalLeads,
              userStats.clusterSupervisors,
              userStats.clerksOfWorks,
            ],
            backgroundColor: ['#10b981', '#3b82f6', '#f59e0b', '#8b5cf6', '#ef4444'],
            borderWidth: 0,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { position: 'bottom' },
        },
      },
    });
  }
};

onMounted(async () => {
  await refreshDashboard();
});

// Cleanup charts on unmount
import { onUnmounted } from 'vue';
onUnmounted(() => {
  if (monthlyChart) monthlyChart.destroy();
  if (userChart) userChart.destroy();
});
</script>