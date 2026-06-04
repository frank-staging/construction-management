<template>
  <div class="max-w-4xl mx-auto">
    <div class="mb-6">
      <h1 class="text-2xl font-semibold text-slate-900 dark:text-white">Register New User</h1>
      <p class="text-slate-600 dark:text-slate-400">Create new user accounts and assign roles</p>
    </div>

    <form @submit.prevent="handleRegister" class="space-y-6">
      <!-- Step 1: Personal Information -->
      <div class="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 p-6">
        <h2 class="text-lg font-semibold mb-4">1. Personal Information</h2>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label class="block text-sm font-medium mb-1">First Name *</label>
            <input 
              v-model="form.firstName" 
              type="text" 
              class="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-emerald-500"
              :class="errors.firstName ? 'border-red-500' : 'border-slate-300'"
              required
            />
            <p v-if="errors.firstName" class="text-xs text-red-500 mt-1">{{ errors.firstName }}</p>
          </div>
          <div>
            <label class="block text-sm font-medium mb-1">Last Name *</label>
            <input 
              v-model="form.lastName" 
              type="text" 
              class="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-emerald-500"
              :class="errors.lastName ? 'border-red-500' : 'border-slate-300'"
              required
            />
            <p v-if="errors.lastName" class="text-xs text-red-500 mt-1">{{ errors.lastName }}</p>
          </div>
          <div>
            <label class="block text-sm font-medium mb-1">Email *</label>
            <input 
              v-model="form.email" 
              type="email" 
              class="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-emerald-500"
              :class="errors.email ? 'border-red-500' : 'border-slate-300'"
              required
            />
            <p v-if="errors.email" class="text-xs text-red-500 mt-1">{{ errors.email }}</p>
          </div>
          <div>
            <label class="block text-sm font-medium mb-1">Phone Number</label>
            <input 
              v-model="form.phoneNumber" 
              type="tel" 
              class="w-full px-3 py-2 border border-slate-300 rounded-lg"
            />
          </div>
          <div>
            <label class="block text-sm font-medium mb-1">Username *</label>
            <input 
              v-model="form.userName" 
              type="text" 
              class="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-emerald-500"
              :class="errors.userName ? 'border-red-500' : 'border-slate-300'"
              required
            />
            <p v-if="errors.userName" class="text-xs text-red-500 mt-1">{{ errors.userName }}</p>
          </div>
          <div>
            <label class="block text-sm font-medium mb-1">Password *</label>
            <div class="relative">
              <input 
                v-model="form.password" 
                :type="showPassword ? 'text' : 'password'" 
                class="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-emerald-500 pr-10"
                :class="errors.password ? 'border-red-500' : 'border-slate-300'"
                required
              />
              <button 
                type="button" 
                @click="showPassword = !showPassword"
                class="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600"
              >
                <EyeIcon v-if="!showPassword" class="w-4 h-4" />
                <EyeOffIcon v-else class="w-4 h-4" />
              </button>
            </div>
            <p v-if="errors.password" class="text-xs text-red-500 mt-1">{{ errors.password }}</p>
          </div>
        </div>
      </div>

      <!-- Step 2: Role Assignment -->
      <div class="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 p-6">
        <h2 class="text-lg font-semibold mb-4">2. Role Assignment</h2>
        <div>
          <label class="block text-sm font-medium mb-1">Select Role *</label>
          <select 
            v-model="selectedRole" 
            class="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-emerald-500"
            :class="errors.role ? 'border-red-500' : 'border-slate-300'"
            required
          >
            <option value="">Select a role</option>
            <option value="SuperAdmin">👑 Super Admin (Full system access)</option>
            <option value="RL">🏢 Regional Lead (RL) - Regional oversight</option>
            <option value="CDH">🏛️ County Director (CDH) - County management</option>
            <option value="TL">🔧 Technical Lead (TL) - Quality control</option>
            <option value="CS">📋 Cluster Supervisor (CS) - Project cluster oversight</option>
            <option value="COW">👷 Clerk of Works (COW) - Field reporting</option>
          </select>
          <p v-if="errors.role" class="text-xs text-red-500 mt-1">{{ errors.role }}</p>
        </div>
        
        <!-- Role Description -->
        <div class="mt-3 p-3 bg-slate-50 dark:bg-slate-700/30 rounded-lg">
          <p class="text-sm text-slate-600 dark:text-slate-400">
            <strong>Role permissions:</strong>
            <span v-if="selectedRole === 'SuperAdmin'"> Full system access, user management, project creation.</span>
            <span v-else-if="selectedRole === 'RL'"> Access to all projects in region, regional analytics.</span>
            <span v-else-if="selectedRole === 'CDH'"> County-level project management and reporting.</span>
            <span v-else-if="selectedRole === 'TL'"> Technical quality control and standards compliance.</span>
            <span v-else-if="selectedRole === 'CS'"> Multiple project oversight and team supervision.</span>
            <span v-else-if="selectedRole === 'COW'"> Daily reporting on assigned projects. Location validation required.</span>
            <span v-else> Select a role to see permissions.</span>
          </p>
        </div>
      </div>

      <!-- Step 3: Project Assignment (ONLY for COW) -->
      <div v-if="selectedRole === 'COW'" class="bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 p-6">
        <h2 class="text-lg font-semibold mb-4">3. Assign to Projects</h2>
        <p class="text-sm text-slate-500 mb-4">
          Select the projects this COW will work on. Their location will be validated against each project's geo-fence.
        </p>
        
        <div v-if="projectsLoading" class="text-center py-4">
          <Loader2Icon class="w-6 h-6 animate-spin mx-auto text-emerald-600" />
          <p class="text-sm text-slate-500 mt-2">Loading projects...</p>
        </div>
        
        <div v-else-if="projects.length === 0" class="text-amber-600 text-sm p-4 bg-amber-50 dark:bg-amber-900/20 rounded-lg">
          ⚠️ No projects available. Please create projects first before assigning COWs.
          <RouterLink to="/dashboard/projects" class="block mt-2 text-emerald-600 hover:text-emerald-700">
            → Create a project
          </RouterLink>
        </div>
        
        <div v-else class="space-y-2 max-h-60 overflow-y-auto border rounded-lg p-2">
          <label 
            v-for="project in projects" 
            :key="project.id" 
            class="flex items-center gap-2 p-2 hover:bg-slate-50 dark:hover:bg-slate-700 rounded cursor-pointer transition-colors"
          >
            <input 
              type="checkbox" 
              v-model="selectedProjects" 
              :value="project.id" 
              class="rounded border-slate-300 text-emerald-600 focus:ring-emerald-500"
            />
            <div class="flex-1">
              <span class="font-medium">{{ project.name }}</span>
              <span class="text-xs text-slate-500 ml-2">({{ project.county || 'No county' }})</span>
            </div>
            <div class="text-xs text-slate-400">
              {{ project.currentPhysicalProgress || 0 }}% complete
            </div>
          </label>
        </div>
        
        <div class="mt-3 text-sm text-slate-500">
          Selected: {{ selectedProjects.length }} project(s)
        </div>
      </div>

      <!-- Summary -->
      <div class="bg-slate-50 dark:bg-slate-700/30 rounded-lg p-4">
        <h3 class="font-medium mb-2">Registration Summary</h3>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-2 text-sm">
          <div><span class="text-slate-500">Name:</span> {{ form.firstName }} {{ form.lastName }}</div>
          <div><span class="text-slate-500">Email:</span> {{ form.email || 'Not set' }}</div>
          <div><span class="text-slate-500">Username:</span> {{ form.userName || 'Not set' }}</div>
          <div><span class="text-slate-500">Role:</span> 
            <span class="font-medium">
              {{ selectedRole === 'SuperAdmin' ? '👑 Super Admin' : '' }}
              {{ selectedRole === 'RL' ? '🏢 Regional Lead' : '' }}
              {{ selectedRole === 'CDH' ? '🏛️ County Director' : '' }}
              {{ selectedRole === 'TL' ? '🔧 Technical Lead' : '' }}
              {{ selectedRole === 'CS' ? '📋 Cluster Supervisor' : '' }}
              {{ selectedRole === 'COW' ? '👷 Clerk of Works' : '' }}
              {{ !selectedRole ? 'Not selected' : '' }}
            </span>
          </div>
          <div v-if="selectedRole === 'COW'">
            <span class="text-slate-500">Projects:</span> {{ selectedProjects.length }} assigned
          </div>
        </div>
      </div>

      <!-- Form Actions -->
      <div class="flex justify-end gap-3">
        <button 
          type="button" 
          @click="$router.back()" 
          class="px-4 py-2 border border-slate-300 rounded-lg hover:bg-slate-50 dark:hover:bg-slate-700 transition-colors"
        >
          Cancel
        </button>
        <button 
          type="submit" 
          :disabled="loading" 
          class="px-4 py-2 bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors flex items-center gap-2"
        >
          <Loader2Icon v-if="loading" class="w-4 h-4 animate-spin" />
          <UserPlusIcon class="w-4 h-4" />
          Register User
        </button>
      </div>
    </form>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useToast } from 'vue-toastification';
import { Loader2Icon, EyeIcon, EyeOffIcon, UserPlusIcon } from 'lucide-vue-next';
import apiClient from '@/services/apiClient';

const router = useRouter();
const toast = useToast();
const loading = ref(false);
const showPassword = ref(false);
const projectsLoading = ref(false);

const form = reactive({
  firstName: '',
  lastName: '',
  email: '',
  phoneNumber: '',
  userName: '',
  password: '',
});

const selectedRole = ref('');
const selectedProjects = ref([]);
const projects = ref([]);
const errors = ref({});

let registeredUserId = null;

const validateForm = () => {
  errors.value = {};
  let isValid = true;

  if (!form.firstName) {
    errors.value.firstName = 'First name is required';
    isValid = false;
  }
  if (!form.lastName) {
    errors.value.lastName = 'Last name is required';
    isValid = false;
  }
  if (!form.email) {
    errors.value.email = 'Email is required';
    isValid = false;
  } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) {
    errors.value.email = 'Invalid email format';
    isValid = false;
  }
  if (!form.userName) {
    errors.value.userName = 'Username is required';
    isValid = false;
  }
  if (!form.password) {
    errors.value.password = 'Password is required';
    isValid = false;
  } else if (form.password.length < 6) {
    errors.value.password = 'Password must be at least 6 characters';
    isValid = false;
  }
  if (!selectedRole.value) {
    errors.value.role = 'Please select a role';
    isValid = false;
  }

  return isValid;
};

// Load projects for COW assignment
const loadProjects = async () => {
  projectsLoading.value = true;
  try {
    const response = await apiClient.get('/Projects?pageSize=100');
    if (response.data.isSuccess) {
      projects.value = response.data.value.items || [];
    }
  } catch (error) {
    console.error('Failed to load projects:', error);
  } finally {
    projectsLoading.value = false;
  }
};

const handleRegister = async () => {
  if (!validateForm()) return;

  loading.value = true;
  try {
    // Step 1: Register user (with default location 0,0,0)
    const registerPayload = {
      email: form.email,
      password: form.password,
      firstName: form.firstName,
      lastName: form.lastName,
      phoneNumber: form.phoneNumber || '',
      userName: form.userName,
      baseLatitude: 0,
      baseLongitude: 0,
      baseRadiusInMeters: 0,
    };

    const registerResponse = await apiClient.post('/Auth/register', registerPayload);
    
    if (!registerResponse.data.isSuccess) {
      toast.error(registerResponse.data.error || 'Registration failed');
      return;
    }

    registeredUserId = registerResponse.data.value.user.id;
    toast.success('✓ User registered successfully');

    // Step 2: Assign role
    const roleResponse = await apiClient.post('/Auth/assign-role', {
      userId: registeredUserId,
      role: selectedRole.value
    });
    
    if (!roleResponse.data.isSuccess) {
      toast.error(`⚠️ User created but role assignment failed: ${roleResponse.data.error}`);
      return;
    }
    toast.success(`✓ Role ${selectedRole.value} assigned`);

    // Step 3: If COW, assign to projects
    if (selectedRole.value === 'COW' && selectedProjects.value.length > 0) {
      let successCount = 0;
      
      for (const projectId of selectedProjects.value) {
        try {
          await apiClient.post('/ProjectAssignments', {
            userId: registeredUserId,
            projectId: projectId,
            role: 'COW'
          });
          successCount++;
        } catch (error) {
          console.error(`Failed to assign project ${projectId}:`, error);
        }
      }
      
      if (successCount > 0) {
        toast.success(`✓ Assigned to ${successCount} project(s)`);
      }
      if (successCount < selectedProjects.value.length) {
        toast.warning(`⚠️ ${selectedProjects.value.length - successCount} project(s) could not be assigned`);
      }
    }

    toast.success(`🎉 User ${form.firstName} ${form.lastName} registered successfully!`);
    router.push('/dashboard/users');
    
  } catch (error) {
    console.error('Registration error:', error);
    toast.error(error.response?.data?.error || 'Registration failed. Please try again.');
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  loadProjects();
});
</script>