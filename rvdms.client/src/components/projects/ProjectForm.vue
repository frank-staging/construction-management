<template>
  <div v-if="isOpen" class="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
    <div class="bg-white dark:bg-slate-800 rounded-xl max-w-2xl w-full max-h-[90vh] overflow-y-auto">
      <div class="p-6">
        <div class="flex justify-between items-center mb-4">
          <h2 class="text-xl font-semibold">Create New Project</h2>
          <button @click="$emit('close')" class="p-1 hover:bg-slate-100 rounded">&times;</button>
        </div>
        
        <form @submit.prevent="handleSubmit">
          <div class="space-y-4">
            <!-- Basic Info -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium mb-1">Project Name *</label>
                <input v-model="form.name" type="text" class="w-full px-3 py-2 border rounded-lg" required />
              </div>
              <div>
                <label class="block text-sm font-medium mb-1">Tender Number</label>
                <input v-model="form.tenderNumber" type="text" class="w-full px-3 py-2 border rounded-lg" />
              </div>
              <div>
                <label class="block text-sm font-medium mb-1">Contractor Name</label>
                <input v-model="form.contractorName" type="text" class="w-full px-3 py-2 border rounded-lg" />
              </div>
              <div>
                <label class="block text-sm font-medium mb-1">Consultant Name</label>
                <input v-model="form.consultantName" type="text" class="w-full px-3 py-2 border rounded-lg" />
              </div>
              <div>
                <label class="block text-sm font-medium mb-1">Contract Sum (KES)</label>
                <input v-model.number="form.contractSum" type="number" class="w-full px-3 py-2 border rounded-lg" />
              </div>
              <div>
                <label class="block text-sm font-medium mb-1">Status</label>
                <select v-model="form.status" class="w-full px-3 py-2 border rounded-lg">
                  <option value="Active">Active</option>
                  <option value="OnHold">On Hold</option>
                  <option value="Completed">Completed</option>
                  <option value="Cancelled">Cancelled</option>
                </select>
              </div>
              <div>
                <label class="block text-sm font-medium mb-1">Start Date</label>
                <input v-model="form.startDate" type="date" class="w-full px-3 py-2 border rounded-lg" />
              </div>
              <div>
                <label class="block text-sm font-medium mb-1">End Date</label>
                <input v-model="form.endDate" type="date" class="w-full px-3 py-2 border rounded-lg" />
              </div>
            </div>

            <!-- Location (IMPORTANT for COW geo-validation) -->
            <div class="border-t pt-4 mt-2">
              <h3 class="font-medium mb-3">Project Location (Geo-fence)</h3>
              <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div>
                  <label class="block text-sm font-medium mb-1">Latitude</label>
                  <input v-model.number="form.latitude" type="number" step="any" class="w-full px-3 py-2 border rounded-lg" placeholder="e.g., 0.5143" />
                </div>
                <div>
                  <label class="block text-sm font-medium mb-1">Longitude</label>
                  <input v-model.number="form.longitude" type="number" step="any" class="w-full px-3 py-2 border rounded-lg" placeholder="e.g., 35.2698" />
                </div>
                <div>
                  <label class="block text-sm font-medium mb-1">Radius (meters)</label>
                  <input v-model.number="form.radiusInMeters" type="number" class="w-full px-3 py-2 border rounded-lg" placeholder="e.g., 500" />
                </div>
              </div>
              <button type="button" @click="getCurrentLocation" class="mt-2 text-sm text-emerald-600 hover:text-emerald-700">
                📍 Use current location
              </button>
            </div>

            <!-- Ward & Cluster -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium mb-1">Ward</label>
                <select v-model="form.wardId" class="w-full px-3 py-2 border rounded-lg">
                  <option value="">Select Ward</option>
                  <option v-for="ward in wards" :key="ward.id" :value="ward.id">{{ ward.name }}</option>
                </select>
              </div>
              <div>
                <label class="block text-sm font-medium mb-1">Cluster</label>
                <select v-model="form.clusterId" class="w-full px-3 py-2 border rounded-lg">
                  <option value="">Select Cluster</option>
                  <option v-for="cluster in clusters" :key="cluster.id" :value="cluster.id">{{ cluster.name }}</option>
                </select>
              </div>
            </div>
          </div>
          
          <div class="flex justify-end gap-3 mt-6 pt-4 border-t">
            <button type="button" @click="$emit('close')" class="px-4 py-2 border rounded-lg">Cancel</button>
            <button type="submit" :disabled="loading" class="px-4 py-2 bg-emerald-600 text-white rounded-lg hover:bg-emerald-700">
              Create Project
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue';
import { useToast } from 'vue-toastification';
import apiClient from '@/services/apiClient';

const props = defineProps(['isOpen']);
const emit = defineEmits(['close', 'created']);
const toast = useToast();
const loading = ref(false);

const form = reactive({
  name: '',
  tenderNumber: '',
  contractorName: '',
  consultantName: '',
  contractSum: 0,
  startDate: '',
  endDate: '',
  status: 'Active',
  latitude: 0,
  longitude: 0,
  radiusInMeters: 500,
  wardId: '',
  clusterId: '',
});

const wards = ref([]);
const clusters = ref([]);

const getCurrentLocation = () => {
  if (navigator.geolocation) {
    navigator.geolocation.getCurrentPosition(
      (position) => {
        form.latitude = position.coords.latitude;
        form.longitude = position.coords.longitude;
        toast.success('Location captured');
      },
      () => toast.error('Unable to get location')
    );
  }
};

const handleSubmit = async () => {
  loading.value = true;
  try {
    const response = await apiClient.post('/Projects', form);
    if (response.data.isSuccess) {
      toast.success('Project created successfully');
      emit('created', response.data.value);
      emit('close');
      
      // Reset form
      Object.keys(form).forEach(key => {
        if (key === 'status') form[key] = 'Active';
        else if (key === 'radiusInMeters') form[key] = 500;
        else if (typeof form[key] === 'string') form[key] = '';
        else form[key] = 0;
      });
    } else {
      toast.error(response.data.error || 'Failed to create project');
    }
  } catch (error) {
    toast.error(error.response?.data?.error || 'Failed to create project');
  } finally {
    loading.value = false;
  }
};

onMounted(async () => {
  // Load wards and clusters for dropdowns
  try {
    const [wardsRes, clustersRes] = await Promise.all([
      apiClient.get('/Wards?pageSize=200'),
      apiClient.get('/Clusters?pageSize=200')
    ]);
    wards.value = wardsRes.data.isSuccess ? wardsRes.data.value.items : [];
    clusters.value = clustersRes.data.isSuccess ? clustersRes.data.value.items : [];
  } catch (error) {
    console.error('Failed to load dropdowns:', error);
  }
});
</script>