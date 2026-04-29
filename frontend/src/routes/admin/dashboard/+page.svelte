<script lang="ts">
	import { onMount } from 'svelte';
	import { api } from '$lib/api';
	import type { PagedMeta, Sale } from '$lib/types';

	let sales: Sale[] = $state([]);
	let pageInfo: PagedMeta = $state({
		pageNumber: 1,
		pageSize: 10,
		totalRecords: 0,
		totalPages: 0
	});

	let isLoading = $state(true);
	let errorMessage = $state('');

	async function fetchSales() {
		isLoading = true;
		try {
			const response = await api.get('/sales', {
				params: {
					pageNumber: 1,
					pageSize: 10
				},
				headers: {
					Accept: 'text/plain'
				}
			});
			const payload = response.data;
			const list = payload?.data ?? payload?.Data ?? payload;

			sales = Array.isArray(list) ? list : [];

			pageInfo = {
				pageNumber: payload?.pageNumber ?? payload?.PageNumber ?? 1,
				pageSize: payload?.pageSize ?? payload?.PageSize ?? sales.length,
				totalRecords: payload?.totalRecords ?? payload?.TotalRecords ?? sales.length,
				totalPages: payload?.totalPages ?? payload?.TotalPages ?? (sales.length > 0 ? 1 : 0)
			};

			errorMessage = '';
		} catch (error: any) {
			errorMessage = error.response?.data?.message || 'Failed to load sales dashboard data.';
			console.error('API Error:', error);
		} finally {
			isLoading = false;
		}
	}

	function formatDate(dateValue: string) {
		const date = new Date(dateValue);
		return Number.isNaN(date.getTime()) ? '-' : date.toLocaleString();
	}

	function formatCurrency(value: number) {
		return new Intl.NumberFormat('en-US', {
			style: 'currency',
			currency: 'USD'
		}).format(value ?? 0);
	}

	const totalRevenue = $derived(sales.reduce((sum, sale) => sum + (sale.totalAmount ?? 0), 0));

	onMount(fetchSales);
</script>

<main class="min-h-screen bg-gray-900 text-white p-8">
	<div class="max-w-6xl mx-auto space-y-8">
		<div class="flex items-center justify-between gap-4">
			<h1 class="text-3xl font-bold text-emerald-400">Sales Dashboard</h1>
			<button
				onclick={fetchSales}
				class="bg-emerald-500 hover:bg-emerald-400 text-gray-900 font-bold py-2 px-4 rounded transition"
			>
				Refresh
			</button>
		</div>

		{#if isLoading}
			<div class="flex justify-center items-center py-20 text-gray-400">
				<span class="animate-pulse">Loading sales data...</span>
			</div>
		{:else if errorMessage}
			<div class="bg-red-500/10 border border-red-500 text-red-400 p-4 rounded">
				{errorMessage}
			</div>
		{:else}
			<div class="grid grid-cols-1 md:grid-cols-3 gap-4">
				<div class="bg-gray-800 border border-gray-700 rounded-lg p-5">
					<p class="text-gray-400 text-sm uppercase tracking-wide">Sales Count</p>
					<p class="text-2xl font-bold text-white mt-2">{sales.length}</p>
				</div>
				<div class="bg-gray-800 border border-gray-700 rounded-lg p-5">
					<p class="text-gray-400 text-sm uppercase tracking-wide">Total Revenue</p>
					<p class="text-2xl font-bold text-emerald-400 mt-2">{formatCurrency(totalRevenue)}</p>
				</div>
				<div class="bg-gray-800 border border-gray-700 rounded-lg p-5">
					<p class="text-gray-400 text-sm uppercase tracking-wide">Page Info</p>
					<p class="text-lg font-semibold text-white mt-2">
						Page {pageInfo.pageNumber} of {pageInfo.totalPages || 1}
					</p>
					<p class="text-sm text-gray-400 mt-1">
						{pageInfo.totalRecords} total records
					</p>
				</div>
			</div>

			{#if sales.length === 0}
				<div class="text-center py-20 text-gray-400 bg-gray-800 rounded-lg border border-gray-700">
					No sales records found.
				</div>
			{:else}
				<div class="bg-gray-800 rounded-lg shadow overflow-hidden border border-gray-700">
					<table class="min-w-full divide-y divide-gray-700">
						<thead class="bg-gray-900/50">
							<tr>
								<th class="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">Receipt</th>
								<th class="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">Sale Date</th>
								<th class="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">Total Amount</th>
								<th class="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">Processed By</th>
							</tr>
						</thead>
						<tbody class="divide-y divide-gray-700 bg-gray-800">
							{#each sales as sale}
								<tr class="hover:bg-gray-750 transition">
									<td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-white">
										{sale.receiptNumber}
									</td>
									<td class="px-6 py-4 whitespace-nowrap text-sm text-gray-400">
										{formatDate(sale.saleDate)}
									</td>
									<td class="px-6 py-4 whitespace-nowrap text-sm text-emerald-400 font-semibold">
										{formatCurrency(sale.totalAmount)}
									</td>
									<td class="px-6 py-4 whitespace-nowrap text-sm text-gray-400">
										{sale.processedByUserId}
									</td>
								</tr>
							{/each}
						</tbody>
					</table>
				</div>
			{/if}
		{/if}
	</div>
</main>
