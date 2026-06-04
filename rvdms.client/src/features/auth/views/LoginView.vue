<template>
  <div
    class="min-h-screen flex items-center justify-center bg-gradient-to-br from-slate-50 to-slate-100 p-4"
  >
    <div class="w-full max-w-md">
      <!-- Header with logos -->
      <div class="text-center mb-6 sm:mb-8">
        <div class="flex items-center justify-center gap-3 sm:gap-4 mb-4 sm:mb-6 flex-wrap">
          <img
            :src="ministryLogo"
            alt="Ministry Logo"
            class="h-12 w-12 sm:h-14 sm:w-14 object-contain"
          />
          <img :src="ahbLogo" alt="AHB Logo" class="h-12 w-12 sm:h-14 sm:w-14 object-contain" />
          <img
            :src="bomaLogo"
            alt="Boma Yangu Logo"
            class="h-10 w-10 sm:h-12 sm:w-12 object-contain"
          />
        </div>
        <h1 class="text-xl sm:text-2xl font-semibold text-slate-900 mb-1 sm:mb-2">
          Affordable Housing Programme
        </h1>
        <p class="text-sm sm:text-base text-slate-600">Nakuru County Pilot – Regional Dashboard</p>
      </div>

      <!-- Login Form Card -->
      <div class="bg-white rounded-xl shadow-lg border border-slate-200 p-6 sm:p-8">
        <h2 class="text-lg sm:text-xl font-semibold mb-5 sm:mb-6 text-slate-900">Secure Login</h2>

        <form @submit.prevent="handleLogin" novalidate class="space-y-4 sm:space-y-5">
          <div>
            <label class="block text-sm mb-1.5 sm:mb-2 text-slate-700">Email Address</label>
            <div class="relative">
              <MailIcon
                class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 sm:w-5 sm:h-5 text-slate-400"
              />
              <input
                v-model="email"
                type="email"
                placeholder="your.email@housing.go.ke"
                class="w-full pl-10 sm:pl-11 pr-4 py-2 sm:py-2.5 text-sm sm:text-base border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-transparent"
                :class="{ 'border-red-500': errors.email }"
                required
              />
            </div>
            <p v-if="errors.email" class="text-xs text-red-600 mt-1">{{ errors.email }}</p>
          </div>

          <div>
            <label class="block text-sm mb-1.5 sm:mb-2 text-slate-700">Password</label>
            <div class="relative">
              <LockIcon
                class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 sm:w-5 sm:h-5 text-slate-400"
              />
              <input
                v-model="password"
                :type="showPassword ? 'text' : 'password'"
                placeholder="Enter your password"
                class="w-full pl-10 sm:pl-11 pr-10 py-2 sm:py-2.5 text-sm sm:text-base border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-transparent"
                :class="{ 'border-red-500': errors.password }"
                required
              />
              <button
                type="button"
                @click="showPassword = !showPassword"
                class="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600"
              >
                <EyeIcon v-if="!showPassword" class="w-4 h-4 sm:w-5 sm:h-5" />
                <EyeOffIcon v-else class="w-4 h-4 sm:w-5 sm:h-5" />
              </button>
            </div>
            <p v-if="errors.password" class="text-xs text-red-600 mt-1">{{ errors.password }}</p>
          </div>

          <!-- Geo-location status -->
          <div
            class="flex items-center gap-2 text-xs sm:text-sm text-slate-600 bg-slate-50 rounded-lg px-3 py-2"
          >
            <MapPinIcon class="w-4 h-4 text-emerald-600 flex-shrink-0" />
            <span class="flex-1">{{ locationStatus }}</span>
          </div>

          <button
            type="submit"
            :disabled="loading"
            class="w-full bg-emerald-600 hover:bg-emerald-700 text-white py-2.5 sm:py-3 rounded-lg transition-colors font-medium text-sm sm:text-base disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
          >
            <Loader2Icon v-if="loading" class="w-4 h-4 animate-spin" />
            {{ loading ? "Verifying location..." : "Login" }}
          </button>
        </form>

        <div class="mt-5 sm:mt-6 pt-5 sm:pt-6 border-t border-slate-200">
          <div class="flex flex-wrap items-center gap-2 text-xs text-slate-500">
            <ShieldIcon class="w-4 h-4" />
            <span>Secured with JWT authentication and geo-limited access</span>
          </div>
        </div>
      </div>

      <p class="text-center text-xs sm:text-sm text-slate-600 mt-4 sm:mt-6">
        Ministry of Lands, Public Works, Housing and Urban Development
      </p>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { useRouter } from "vue-router";
import { useToast } from "vue-toastification";
import { useAuthStore } from "@/stores/AuthStore";
import {
  Mail as MailIcon,
  Lock as LockIcon,
  Shield as ShieldIcon,
  MapPin as MapPinIcon,
  Eye as EyeIcon,
  EyeOff as EyeOffIcon,
  Loader2 as Loader2Icon,
} from "lucide-vue-next";

import ministryLogo from "@/assets/logos/ministry.jpg";
import ahbLogo from "@/assets/logos/ahb.jpg";
import bomaLogo from "@/assets/logos/boma-yangu.jpg";

const router = useRouter();
const authStore = useAuthStore();
const toast = useToast();

const email = ref("");
const password = ref("");
const showPassword = ref(false);
const loading = ref(false);
const locationStatus = ref("Detecting your location...");
const errors = ref({});

onMounted(() => {
  // Check if geolocation is available
  if (!navigator.geolocation) {
    locationStatus.value = "Geolocation is not supported by your browser";
  } else {
    locationStatus.value = "Location access required for login";
  }
});

const validateForm = () => {
  errors.value = {};
  let isValid = true;

  if (!email.value) {
    errors.value.email = "Email is required";
    isValid = false;
  } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.value)) {
    errors.value.email = "Please enter a valid email address";
    isValid = false;
  }

  if (!password.value) {
    errors.value.password = "Password is required";
    isValid = false;
  } else if (password.value.length < 6) {
    errors.value.password = "Password must be at least 6 characters";
    isValid = false;
  }

  return isValid;
};

const handleLogin = async () => {
  if (!validateForm()) return;

  loading.value = true;
  locationStatus.value = "Getting your location...";

  try {
    // 1️⃣ Get location
    const position = await authStore.getCurrentPosition();
    const { latitude, longitude } = position.coords;

    // 2️⃣ Call login WITH location
    const result = await authStore.login(email.value, password.value, latitude, longitude);

    if (!result.success) {
      toast.error(result.error || "Login failed");
      return;
    }

    toast.success("Login successful 🚀");

    // 3️⃣ Redirect based on role (UPDATED with SuperAdmin)
    const role = authStore.userRole;

    switch (role) {
      case "SuperAdmin":
        // SuperAdmin goes to full analytics dashboard
        router.push("/dashboard");
        break;

      case "RL":
      case "CDH":
      case "TL":
      case "CS":
        // All leadership roles go to the leadership dashboard
        router.push("/dashboard");
        break;

      case "COW":
        // Clerk of Works goes to their specific dashboard
        router.push("/dashboard/clerk");
        break;

      default:
        // Fallback for any other role
        router.push("/dashboard");
        break;
    }
  } catch (error) {
    toast.error("Location access is required. Please enable GPS and try again.");
    locationStatus.value = "Location access denied";
    console.error("Login error:", error);
  } finally {
    loading.value = false;
  }
};
</script>