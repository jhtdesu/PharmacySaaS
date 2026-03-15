<script lang="ts">
  let appName: string = "PharmacyPro SaaS";
  let version: number = 1.0;
</script>

<main class="min-h-screen bg-gray-900 text-white flex flex-col items-center justify-center">
  <h1 class="text-5xl font-bold text-emerald-400">{appName}</h1>
  <p class="mt-4 text-gray-400">Point of Sale Terminal v{version}</p>
  
  <button class="mt-8 px-6 py-2 bg-emerald-500 text-gray-900 font-bold rounded hover:bg-emerald-400 transition">
    Start Shift
  </button>
</main>

<style>
</style>