<script lang="ts">
    import { onMount } from 'svelte';
    import { api } from '$lib/api';
    import type { Medicine } from '$lib/types';

    let medicines: Medicine[] = $state([]);
    let isLoading = $state(true);
    let errorMessage = $state('');
    
    async function fetchMedicines() {
        try {
            const response = await api.get('/medicines/low-stock');
            const payload = response.data;
            const list = payload?.data ?? payload?.Data ?? payload;

            medicines = Array.isArray(list) ? list : [];
            errorMessage = '';
        } catch (error: any) {
            errorMessage = error.response?.data?.message || 'Network error occurred.';
            console.error("API Error:", error);
        } finally {
            isLoading = false;
        }
    }

    onMount(fetchMedicines);
</script>

<main class="min-h-screen bg-gray-900 text-white p-8">
    <div class="max-w-6xl mx-auto">

        {#if isLoading}
            <div class="flex justify-center items-center py-20 text-gray-400">
                <span class="animate-pulse">Loading inventory...</span>
            </div>
        {:else if errorMessage}
            <div class="bg-red-500/10 border border-red-500 text-red-400 p-4 rounded">
                {errorMessage}
            </div>
        {:else if medicines.length == 0}
            <div class="text-center py-20 text-gray-400 bg-gray-800 rounded-lg border border-gray-700">
                No medicines found. Add some stock to get started.
            </div>
        {:else}
            <div class="bg-gray-800 rounded-lg shadow overflow-hidden border border-gray-700">
                <table class="min-w-full divide-y divide-gray-700">
                    <thead class="bg-gray-900/50">
                        <tr>
                            <th class="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">Name</th>
                            <th class="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">SKU</th>
                            <th class="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">Active Ingredient</th>
                            <th class="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">Unit</th>
                        </tr>
                    </thead>
                    <tbody class="divide-y divide-gray-700 bg-gray-800">
                        {#each medicines as medicine}
                            <tr class="hover:bg-gray-750 transition">
                                <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-white">
                                    {medicine.name}
                                </td>
                                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-400">
                                    {medicine.sku}
                                </td>
                                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-400">
                                    {medicine.activeIngredient}
                                </td>
                                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-400">
                                    {medicine.unit}
                                </td>
                            </tr>
                        {/each}
                    </tbody>
                </table>
            </div>
        {/if}
    </div>
</main>