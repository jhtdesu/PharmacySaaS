<script lang="ts">
    import { api } from '$lib/api';

    interface Medicine {
        id: string;
        name: string;
        sku: string;
        unit: string;
    }

    let medicines: Medicine[] = $state([]);
    let isLoading = $state(true);
    let errorMessage = $state('');
    
    async function fetchMedicines() {
        try {
            const response = await api.get('/medicines');
            medicines = response.data.data;
        } catch (error: any) {
            errorMessage = error.response?.data?.message || 'Network error occurred.';
            console.error("API Error:", error);
        } finally {
            isLoading = false;
        }
    }

    fetchMedicines();
</script>

<main class="min-h-screen bg-gray-900 text-white p-8">
    <div class="max-w-6xl mx-auto">
        <div class="flex justify-between items-center mb-8">
            <h1 class="text-3xl font-bold text-emerald-400">Medicines Inventory</h1>
            <button class="bg-emerald-500 hover:bg-emerald-400 text-gray-900 font-bold py-2 px-4 rounded transition">
                + Add Medicine
            </button>
        </div>

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
                            <th class="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">Unit</th>
                            <th class="px-6 py-3 text-right text-xs font-medium text-gray-400 uppercase tracking-wider">Actions</th>
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
                                    {medicine.unit}
                                </td>
                                <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                                    <button class="text-emerald-400 hover:text-emerald-300 transition mr-3">Edit</button>
                                    <button class="text-red-400 hover:text-red-300 transition">Delete</button>
                                </td>
                            </tr>
                        {/each}
                    </tbody>
                </table>
            </div>
        {/if}
    </div>
</main>