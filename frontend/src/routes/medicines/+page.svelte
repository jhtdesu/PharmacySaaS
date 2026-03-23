<script lang="ts">
    import { onMount } from 'svelte';
    import { api } from '$lib/api';
    import type { Medicine } from '$lib/types';

    let medicines: Medicine[] = $state([]);
    let isLoading = $state(true);
    let errorMessage = $state('');

    let showModal = $state(false);
    let isSubmitting = $state(false);
    let editingMedicineId = $state<string | null>(null);

    let newMedicine = $state({
        name: '',
        sku: '',
        activeIngredient: '',
        unit: ''
    });

    function resetMedicineForm() {
        newMedicine = {
            name: '',
            sku: '',
            activeIngredient: '',
            unit: ''
        };
        editingMedicineId = null;
    }

    function openCreateModal() {
        resetMedicineForm();
        showModal = true;
    }

    function openEditModal(medicine: Medicine) {
        editingMedicineId = medicine.id;
        newMedicine = {
            name: medicine.name,
            sku: medicine.sku,
            activeIngredient: medicine.activeIngredient,
            unit: medicine.unit
        };
        showModal = true;
    }

    function closeModal() {
        showModal = false;
        resetMedicineForm();
    }

    async function handleSubmitMedicine() {
        if (editingMedicineId) {
            await handleUpdateMedicine(editingMedicineId);
            return;
        }

        await handleCreateMedicine();
    }
    
    async function fetchMedicines() {
        try {
            const response = await api.get('/medicines');
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

    async function handleCreateMedicine() {
        isSubmitting = true;
        try {
            const response = await api.post('/medicines', newMedicine);
            const result = response.data;

            if (result.success === true) {
                closeModal();
                await fetchMedicines(); 
            } else {
                alert("Error: " + result.message);
            }
        } catch (error: any) {
            alert("Failed to create medicine: " + (error.response?.data?.message || error.message));
        } finally {
            isSubmitting = false;
        }
    }

    async function handleUpdateMedicine(medicineId: string) {
        isSubmitting = true;
        try {
            const response = await api.put(`/medicines/${medicineId}`, newMedicine);
            const result = response.data;

            if (result.success === true) {
                closeModal();
                await fetchMedicines(); 
            } else {
                alert("Error: " + result.message);
            }
        } catch (error: any) {
            alert("Failed to update medicine: " + (error.response?.data?.message || error.message));
        } finally {
            isSubmitting = false;
        }
    }

    async function handleDeleteMedicine(medicineId: string) {
        const shouldDelete = confirm('Delete this medicine?');
        if (!shouldDelete) return;

        try {
            await api.delete(`/medicines/${medicineId}`);
            await fetchMedicines();
        } catch (error: any) {
            alert("Failed to delete medicine: " + (error.response?.data?.message || error.message));
        }
    }

    onMount(fetchMedicines);
</script>

<main class="min-h-screen bg-gray-900 text-white p-8">
    <div class="max-w-6xl mx-auto">
        <div class="flex justify-between items-center mb-8">
            <h1 class="text-3xl font-bold text-emerald-400">Medicines Inventory</h1>
            <button onclick={openCreateModal} class="bg-emerald-500 hover:bg-emerald-400 text-gray-900 font-bold py-2 px-4 rounded transition">
            
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
                            <th class="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">Active Ingredient</th>
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
                                    {medicine.activeIngredient}
                                </td>
                                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-400">
                                    {medicine.unit}
                                </td>
                                <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                                    <button onclick={() => openEditModal(medicine)} class="text-emerald-400 hover:text-emerald-300 transition mr-3">Edit</button>
                                    <button onclick={() => handleDeleteMedicine(medicine.id)} class="text-red-400 hover:text-red-300 transition">Delete</button>
                                </td>
                            </tr>
                        {/each}
                    </tbody>
                </table>
            </div>
        {/if}
        
        {#if showModal}
        <div class="fixed inset-0 bg-black/70 flex items-center justify-center z-50 p-4">
            <div class="bg-gray-800 border border-gray-700 rounded-xl shadow-2xl p-6 w-full max-w-md">
                <h2 class="text-2xl font-bold text-white mb-6">{editingMedicineId ? 'Edit Medicine' : 'Add New Medicine'}</h2>
                
                <form onsubmit={(event) => { event.preventDefault(); handleSubmitMedicine(); }} class="space-y-4">
                    <div>
                        <label for="medicine-name" class="block text-sm font-medium text-gray-400 mb-1">Medicine Name</label>
                        <input id="medicine-name" type="text" bind:value={newMedicine.name} required 
                            class="w-full rounded-md border-gray-600 bg-gray-700 text-white shadow-sm focus:border-emerald-500 focus:ring-emerald-500 px-4 py-2" 
                            placeholder="e.g. Paracetamol 500mg" />
                    </div>
                    
                    <div>
                        <label for="medicine-sku" class="block text-sm font-medium text-gray-400 mb-1">SKU (Barcode/Identifier)</label>
                        <input id="medicine-sku" type="text" bind:value={newMedicine.sku} required 
                            class="w-full rounded-md border-gray-600 bg-gray-700 text-white shadow-sm focus:border-emerald-500 focus:ring-emerald-500 px-4 py-2" 
                            placeholder="e.g. PARA-500-BOX" />
                    </div>

                    <div>
                        <label for="medicine-active-ingredient" class="block text-sm font-medium text-gray-400 mb-1">Active Ingredient</label>
                        <input id="medicine-active-ingredient" type="text" bind:value={newMedicine.activeIngredient} required 
                            class="w-full rounded-md border-gray-600 bg-gray-700 text-white shadow-sm focus:border-emerald-500 focus:ring-emerald-500 px-4 py-2" 
                            placeholder="e.g. Paracetamol" />
                    </div>

                    <div>
                        <label for="medicine-unit" class="block text-sm font-medium text-gray-400 mb-1">Unit Type</label>
                        <input id="medicine-unit" type="text" bind:value={newMedicine.unit} required 
                            class="w-full rounded-md border-gray-600 bg-gray-700 text-white shadow-sm focus:border-emerald-500 focus:ring-emerald-500 px-4 py-2" 
                            placeholder="e.g. Box, Bottle, Blister" />
                    </div>

                    <div class="flex justify-end space-x-3 mt-8">
                        <button type="button" onclick={closeModal} 
                            class="px-4 py-2 bg-gray-700 text-gray-300 rounded hover:bg-gray-600 transition">
                            Cancel
                        </button>
                        <button type="submit" disabled={isSubmitting} 
                            class="px-4 py-2 bg-emerald-500 text-gray-900 font-bold rounded hover:bg-emerald-400 transition disabled:opacity-50">
                            {isSubmitting ? 'Saving...' : editingMedicineId ? 'Update Medicine' : 'Save Medicine'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    {/if}
    </div>
</main>