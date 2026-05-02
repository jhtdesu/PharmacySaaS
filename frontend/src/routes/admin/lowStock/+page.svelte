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
			console.error('API Error:', error);
		} finally {
			isLoading = false;
		}
	}

	onMount(fetchMedicines);
</script>

<main class="min-h-screen bg-gray-900 p-8 text-white">
	<div class="mx-auto max-w-6xl">
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
							</tr>
						{/each}
					</tbody>
				</table>
			</div>
		{/if}
	</div>
</main>
