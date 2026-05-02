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
		unit: '',
		imageUrl: ''
	});

	function resetMedicineForm() {
		newMedicine = {
			name: '',
			sku: '',
			activeIngredient: '',
			unit: '',
			imageUrl: ''
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
			unit: medicine.unit,
			imageUrl: medicine.imageUrl || ''
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
			console.error('API Error:', error);
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
				alert('Error: ' + result.message);
			}
		} catch (error: any) {
			alert('Failed to create medicine: ' + (error.response?.data?.message || error.message));
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
				alert('Error: ' + result.message);
			}
		} catch (error: any) {
			alert('Failed to update medicine: ' + (error.response?.data?.message || error.message));
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
			alert('Failed to delete medicine: ' + (error.response?.data?.message || error.message));
		}
	}

	onMount(fetchMedicines);
</script>

<main class="min-h-screen bg-gray-900 p-8 text-white">
	<div class="mx-auto max-w-6xl">
		<div class="mb-8 flex items-center justify-between">
			<h1 class="text-3xl font-bold text-emerald-400">Medicines Inventory</h1>
			<button
				onclick={openCreateModal}
				class="rounded bg-emerald-500 px-4 py-2 font-bold text-gray-900 transition hover:bg-emerald-400"
			>
				+ Add Medicine
			</button>
		</div>

		{#if isLoading}
			<div class="flex items-center justify-center py-20 text-gray-400">
				<span class="animate-pulse">Loading inventory...</span>
			</div>
		{:else if errorMessage}
			<div class="rounded border border-red-500 bg-red-500/10 p-4 text-red-400">
				{errorMessage}
			</div>
		{:else if medicines.length == 0}
			<div class="rounded-lg border border-gray-700 bg-gray-800 py-20 text-center text-gray-400">
				No medicines found. Add some stock to get started.
			</div>
		{:else}
			<div class="overflow-hidden rounded-lg border border-gray-700 bg-gray-800 shadow">
				<table class="min-w-full divide-y divide-gray-700">
					<thead class="bg-gray-900/50">
						<tr>
							<th
								class="px-6 py-3 text-left text-xs font-medium tracking-wider text-gray-400 uppercase"
								>Image</th
							>
							<th
								class="px-6 py-3 text-left text-xs font-medium tracking-wider text-gray-400 uppercase"
								>Name</th
							>
							<th
								class="px-6 py-3 text-left text-xs font-medium tracking-wider text-gray-400 uppercase"
								>SKU</th
							>
							<th
								class="px-6 py-3 text-left text-xs font-medium tracking-wider text-gray-400 uppercase"
								>Active Ingredient</th
							>
							<th
								class="px-6 py-3 text-left text-xs font-medium tracking-wider text-gray-400 uppercase"
								>Unit</th
							>
							<th
								class="px-6 py-3 text-right text-xs font-medium tracking-wider text-gray-400 uppercase"
								>Actions</th
							>
						</tr>
					</thead>
					<tbody class="divide-y divide-gray-700 bg-gray-800">
						{#each medicines as medicine}
							<tr class="hover:bg-gray-750 transition">
								<td class="px-6 py-4 whitespace-nowrap">
									{#if medicine.imageUrl}
										<img
											src={medicine.imageUrl}
											alt={medicine.name}
											class="h-10 w-10 rounded object-cover shadow"
										/>
									{:else}
										<div
											class="flex h-10 w-10 items-center justify-center rounded bg-gray-700 text-xs text-gray-500 shadow"
										>
											N/A
										</div>
									{/if}
								</td>
								<td class="px-6 py-4 text-sm font-medium whitespace-nowrap text-white">
									{medicine.name}
								</td>
								<td class="px-6 py-4 text-sm whitespace-nowrap text-gray-400">
									{medicine.sku}
								</td>
								<td class="px-6 py-4 text-sm whitespace-nowrap text-gray-400">
									{medicine.activeIngredient}
								</td>
								<td class="px-6 py-4 text-sm whitespace-nowrap text-gray-400">
									{medicine.unit}
								</td>
								<td class="px-6 py-4 text-right text-sm font-medium whitespace-nowrap">
									<button
										onclick={() => openEditModal(medicine)}
										class="mr-3 text-emerald-400 transition hover:text-emerald-300">Edit</button
									>
									<button
										onclick={() => handleDeleteMedicine(medicine.id)}
										class="text-red-400 transition hover:text-red-300">Delete</button
									>
								</td>
							</tr>
						{/each}
					</tbody>
				</table>
			</div>
		{/if}

		{#if showModal}
			<div class="fixed inset-0 z-50 flex items-center justify-center bg-black/70 p-4">
				<div class="w-full max-w-md rounded-xl border border-gray-700 bg-gray-800 p-6 shadow-2xl">
					<h2 class="mb-6 text-2xl font-bold text-white">
						{editingMedicineId ? 'Edit Medicine' : 'Add New Medicine'}
					</h2>

					<form
						onsubmit={(event) => {
							event.preventDefault();
							handleSubmitMedicine();
						}}
						class="space-y-4"
					>
						<div>
							<label for="medicine-name" class="mb-1 block text-sm font-medium text-gray-400"
								>Medicine Name</label
							>
							<input
								id="medicine-name"
								type="text"
								bind:value={newMedicine.name}
								required
								class="w-full rounded-md border-gray-600 bg-gray-700 px-4 py-2 text-white shadow-sm focus:border-emerald-500 focus:ring-emerald-500"
								placeholder="e.g. Paracetamol 500mg"
							/>
						</div>

						<div>
							<label for="medicine-sku" class="mb-1 block text-sm font-medium text-gray-400"
								>SKU (Barcode/Identifier)</label
							>
							<input
								id="medicine-sku"
								type="text"
								bind:value={newMedicine.sku}
								required
								class="w-full rounded-md border-gray-600 bg-gray-700 px-4 py-2 text-white shadow-sm focus:border-emerald-500 focus:ring-emerald-500"
								placeholder="e.g. PARA-500-BOX"
							/>
						</div>

						<div>
							<label
								for="medicine-active-ingredient"
								class="mb-1 block text-sm font-medium text-gray-400">Active Ingredient</label
							>
							<input
								id="medicine-active-ingredient"
								type="text"
								bind:value={newMedicine.activeIngredient}
								required
								class="w-full rounded-md border-gray-600 bg-gray-700 px-4 py-2 text-white shadow-sm focus:border-emerald-500 focus:ring-emerald-500"
								placeholder="e.g. Paracetamol"
							/>
						</div>

						<div>
							<label for="medicine-unit" class="mb-1 block text-sm font-medium text-gray-400"
								>Unit Type</label
							>
							<input
								id="medicine-unit"
								type="text"
								bind:value={newMedicine.unit}
								required
								class="w-full rounded-md border-gray-600 bg-gray-700 px-4 py-2 text-white shadow-sm focus:border-emerald-500 focus:ring-emerald-500"
								placeholder="e.g. Box, Bottle, Blister"
							/>
						</div>

						<div>
							<label for="medicine-image-url" class="mb-1 block text-sm font-medium text-gray-400"
								>Image URL</label
							>
							<input
								id="medicine-image-url"
								type="url"
								bind:value={newMedicine.imageUrl}
								class="w-full rounded-md border-gray-600 bg-gray-700 px-4 py-2 text-white shadow-sm focus:border-emerald-500 focus:ring-emerald-500"
								placeholder="https://example.com/image.png"
							/>
						</div>

						<div class="mt-8 flex justify-end space-x-3">
							<button
								type="button"
								onclick={closeModal}
								class="rounded bg-gray-700 px-4 py-2 text-gray-300 transition hover:bg-gray-600"
							>
								Cancel
							</button>
							<button
								type="submit"
								disabled={isSubmitting}
								class="rounded bg-emerald-500 px-4 py-2 font-bold text-gray-900 transition hover:bg-emerald-400 disabled:opacity-50"
							>
								{isSubmitting
									? 'Saving...'
									: editingMedicineId
										? 'Update Medicine'
										: 'Save Medicine'}
							</button>
						</div>
					</form>
				</div>
			</div>
		{/if}
	</div>
</main>
