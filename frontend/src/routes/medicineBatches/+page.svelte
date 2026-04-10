<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api';
  import type { Medicine, MedicineBatch } from '$lib/types';

  let batches: MedicineBatch[] = $state([]);
  let medicines: Medicine[] = $state([]);
  let isLoading = $state(true);
  let errorMessage = $state('');
  let showModal = $state(false);
  let isSubmitting = $state(false);
  let editingBatchId = $state<string | null>(null);

  let batchForm = $state({
    medicineId: '',
    batchNumber: '',
    expiryDate: '',
    quantity: 0
  });

  function resetBatchForm() {
    batchForm = {
      medicineId: medicines[0]?.id ?? '',
      batchNumber: '',
      expiryDate: '',
      quantity: 0
    };
    editingBatchId = null;
  }

  function openCreateModal() {
    resetBatchForm();
    showModal = true;
  }

  function openEditModal(batch: MedicineBatch) {
    editingBatchId = batch.id;
    batchForm = {
      medicineId: '',
      batchNumber: batch.batchNumber,
      expiryDate: batch.expiryDate ? batch.expiryDate.split('T')[0] : '',
      quantity: batch.quantity
    };
    showModal = true;
  }

  function closeModal() {
    showModal = false;
    resetBatchForm();
  }

  async function handleSubmitBatch() {
    if (editingBatchId) {
      await handleUpdateBatch(editingBatchId);
      return;
    }

    await handleCreateBatch();
  }

  async function fetchMedicines() {
    try {
      const response = await api.get('/medicines');
      const payload = response.data;
      const list = payload?.data ?? payload?.Data ?? payload;

      medicines = Array.isArray(list) ? list : [];

      if (!batchForm.medicineId && medicines.length > 0) {
        batchForm = { ...batchForm, medicineId: medicines[0].id };
      }
    } catch (error) {
      console.error('Failed to load medicines:', error);
    }
  }

  async function fetchMedicineBatches() {
    try {
      const response = await api.get('/batches');
      const payload = response.data;
      const list = payload?.data ?? payload?.Data ?? payload;

      batches = Array.isArray(list) ? list : [];
      errorMessage = '';
    } catch (error: any) {
      errorMessage = error.response?.data?.message || 'Network error occurred.';
      console.error('API Error:', error);
    } finally {
      isLoading = false;
    }
  }

  async function handleCreateBatch() {
    if (!batchForm.medicineId) {
      alert('Please select a medicine.');
      return;
    }

    isSubmitting = true;
    try {
      await api.post(`/batches/${batchForm.medicineId}`, {
        batchNumber: batchForm.batchNumber,
        expiryDate: batchForm.expiryDate,
        quantity: Number(batchForm.quantity)
      });

      closeModal();
      await fetchMedicineBatches();
    } catch (error: any) {
      alert('Failed to create batch: ' + (error.response?.data?.message || error.message));
    } finally {
      isSubmitting = false;
    }
  }

  async function handleUpdateBatch(batchId: string) {
    isSubmitting = true;
    try {
      await api.put(`/batches/${batchId}`, {
        batchNumber: batchForm.batchNumber,
        expiryDate: batchForm.expiryDate,
        quantity: Number(batchForm.quantity)
      });

      closeModal();
      await fetchMedicineBatches();
    } catch (error: any) {
      alert('Failed to update batch: ' + (error.response?.data?.message || error.message));
    } finally {
      isSubmitting = false;
    }
  }

  async function handleDeleteBatch(batchId: string) {
    const shouldDelete = confirm('Delete this batch?');
    if (!shouldDelete) return;

    try {
      await api.delete(`/batches/${batchId}`);
      await fetchMedicineBatches();
    } catch (error: any) {
      alert('Failed to delete batch: ' + (error.response?.data?.message || error.message));
    }
  }

  onMount(async () => {
    await Promise.all([fetchMedicines(), fetchMedicineBatches()]);
  });

  function formatDate(dateValue: string) {
    const date = new Date(dateValue);
    return Number.isNaN(date.getTime()) ? '-' : date.toLocaleDateString();
  }
</script>

<main class="min-h-screen bg-gray-900 text-white p-8">
  <div class="max-w-6xl mx-auto">
    <div class="flex justify-between items-center mb-8">
      <h1 class="text-3xl font-bold text-emerald-400">Medicine Batches</h1>
      <button onclick={openCreateModal} class="bg-emerald-500 hover:bg-emerald-400 text-gray-900 font-bold py-2 px-4 rounded transition">
        + Add Batch
      </button>
    </div>

    {#if isLoading}
      <div class="flex justify-center items-center py-20 text-gray-400">
        <span class="animate-pulse">Loading batches...</span>
      </div>
    {:else if errorMessage}
      <div class="bg-red-500/10 border border-red-500 text-red-400 p-4 rounded">
        {errorMessage}
      </div>
    {:else if batches.length === 0}
      <div class="text-center py-20 text-gray-400 bg-gray-800 rounded-lg border border-gray-700">
        No medicine batches found.
      </div>
    {:else}
      <div class="bg-gray-800 rounded-lg shadow overflow-hidden border border-gray-700">
        <table class="min-w-full divide-y divide-gray-700">
          <thead class="bg-gray-900/50">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">Batch Number</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">Expiry Date</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">Quantity</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-gray-400 uppercase tracking-wider">Actions</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-700 bg-gray-800">
            {#each batches as batch}
              <tr class="hover:bg-gray-750 transition">
                <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-white">
                  {batch.batchNumber}
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-400">
                  {formatDate(batch.expiryDate)}
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-400">
                  {batch.quantity}
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                  <button onclick={() => openEditModal(batch)} class="text-emerald-400 hover:text-emerald-300 transition mr-3">Edit</button>
                  <button onclick={() => handleDeleteBatch(batch.id)} class="text-red-400 hover:text-red-300 transition">Delete</button>
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
          <h2 class="text-2xl font-bold text-white mb-6">{editingBatchId ? 'Edit Batch' : 'Add New Batch'}</h2>

          <form
            onsubmit={(event) => {
              event.preventDefault();
              handleSubmitBatch();
            }}
            class="space-y-4"
          >
            {#if !editingBatchId}
              <div>
                <label for="batch-medicine" class="block text-sm font-medium text-gray-400 mb-1">Medicine</label>
                <select
                  id="batch-medicine"
                  bind:value={batchForm.medicineId}
                  required
                  class="w-full rounded-md border-gray-600 bg-gray-700 text-white shadow-sm focus:border-emerald-500 focus:ring-emerald-500 px-4 py-2"
                >
                  {#if medicines.length === 0}
                    <option value="" disabled selected>No medicines available</option>
                  {:else}
                    {#each medicines as medicine}
                      <option value={medicine.id}>{medicine.name}</option>
                    {/each}
                  {/if}
                </select>
              </div>
            {/if}

            <div>
              <label for="batch-number" class="block text-sm font-medium text-gray-400 mb-1">Batch Number</label>
              <input
                id="batch-number"
                type="text"
                bind:value={batchForm.batchNumber}
                required
                class="w-full rounded-md border-gray-600 bg-gray-700 text-white shadow-sm focus:border-emerald-500 focus:ring-emerald-500 px-4 py-2"
                placeholder="e.g. BATCH-2026-001"
              />
            </div>

            <div>
              <label for="batch-expiry" class="block text-sm font-medium text-gray-400 mb-1">Expiry Date</label>
              <input
                id="batch-expiry"
                type="date"
                bind:value={batchForm.expiryDate}
                required
                class="w-full rounded-md border-gray-600 bg-gray-700 text-white shadow-sm focus:border-emerald-500 focus:ring-emerald-500 px-4 py-2"
              />
            </div>

            <div>
              <label for="batch-quantity" class="block text-sm font-medium text-gray-400 mb-1">Quantity</label>
              <input
                id="batch-quantity"
                type="number"
                min="0"
                bind:value={batchForm.quantity}
                required
                class="w-full rounded-md border-gray-600 bg-gray-700 text-white shadow-sm focus:border-emerald-500 focus:ring-emerald-500 px-4 py-2"
              />
            </div>

            <div class="flex justify-end space-x-3 mt-8">
              <button type="button" onclick={closeModal} class="px-4 py-2 bg-gray-700 text-gray-300 rounded hover:bg-gray-600 transition">
                Cancel
              </button>
              <button
                type="submit"
                disabled={isSubmitting}
                class="px-4 py-2 bg-emerald-500 text-gray-900 font-bold rounded hover:bg-emerald-400 transition disabled:opacity-50"
              >
                {isSubmitting ? 'Saving...' : editingBatchId ? 'Update Batch' : 'Save Batch'}
              </button>
            </div>
          </form>
        </div>
      </div>
    {/if}
  </div>
</main>