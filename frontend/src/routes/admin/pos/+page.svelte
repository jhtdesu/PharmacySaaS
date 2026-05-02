<script lang="ts">
	import { onMount } from 'svelte';
	import { api } from '$lib/api';
	import type { Medicine, MedicineCheckout } from '$lib/types';

	type CheckoutOrderInfo = {
		orderId?: string;
		fullName?: string;
		amount?: number;
		orderInfo?: string;
		OrderId?: string;
		FullName?: string;
		Amount?: number;
		OrderInfo?: string;
	};

	type PaymentLinkResponse = {
		payUrl?: string;
		PayUrl?: string;
	};

	let medicines: Medicine[] = $state([]);
	let cart: (MedicineCheckout & { name: string })[] = $state([]);
	let selectedMedicineId = $state('');
	let selectedQuantity = $state(1);
	let isLoading = $state(true);
	let errorMessage = $state('');
	let isCheckingOut = $state(false);

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

	function addToCart() {
		if (!selectedMedicineId || selectedQuantity <= 0) {
			alert('Please select a medicine and quantity');
			return;
		}

		const medicine = medicines.find((m) => m.id === selectedMedicineId);
		if (!medicine) return;

		// Check if medicine already in cart
		const existingItem = cart.find((item) => item.medicineId === selectedMedicineId);
		if (existingItem) {
			existingItem.quantity += selectedQuantity;
		} else {
			cart.push({
				medicineId: selectedMedicineId,
				quantity: selectedQuantity,
				name: medicine.name
			});
		}

		// Reset form
		selectedMedicineId = '';
		selectedQuantity = 1;
		cart = cart; // Trigger reactivity
	}

	function removeFromCart(medicineId: string) {
		cart = cart.filter((item) => item.medicineId !== medicineId);
	}

	function updateQuantity(medicineId: string, newQuantity: number) {
		const item = cart.find((item) => item.medicineId === medicineId);
		if (item) {
			if (newQuantity <= 0) {
				removeFromCart(medicineId);
			} else {
				item.quantity = newQuantity;
				cart = cart; // Trigger reactivity
			}
		}
	}

	async function checkoutCart() {
		if (cart.length === 0) {
			alert('Cart is empty');
			return;
		}

		isCheckingOut = true;
		try {
			const checkoutResponse = await api.post(`medicines/checkout`, {
				items: cart.map((item) => ({
					medicineId: item.medicineId,
					quantity: item.quantity
				}))
			});

			const checkoutPayload = checkoutResponse.data;
			const checkoutData: CheckoutOrderInfo | undefined =
				checkoutPayload?.data ?? checkoutPayload?.Data;

			if (!checkoutData) {
				throw new Error('Checkout response is missing order data.');
			}

			const persistedFullName = localStorage.getItem('user_full_name') ?? '';
			const resolvedFullName =
				(checkoutData.fullName ?? checkoutData.FullName ?? persistedFullName).trim() ||
				'Khách hàng';

			const orderInfoPayload = {
				orderId: checkoutData.orderId ?? checkoutData.OrderId,
				fullName: resolvedFullName,
				amount: checkoutData.amount ?? checkoutData.Amount,
				orderInfo: checkoutData.orderInfo ?? checkoutData.OrderInfo
			};

			const paymentResponse = await api.post(`momo`, orderInfoPayload);
			const paymentPayload = paymentResponse.data;
			const paymentData: PaymentLinkResponse | undefined =
				paymentPayload?.data ?? paymentPayload?.Data;
			const paymentUrl = paymentData?.payUrl ?? paymentData?.PayUrl;

			if (!paymentUrl) {
				throw new Error('Payment URL not returned by payment service.');
			}

			cart = [];
			await fetchMedicines();
			window.location.href = paymentUrl;
		} catch (error: any) {
			alert(error.response?.data?.message || 'Failed to checkout medicines.');
			console.error('Checkout Error:', error);
		} finally {
			isCheckingOut = false;
		}
	}

	onMount(fetchMedicines);
</script>

<main class="min-h-screen bg-gray-900 p-8 text-white">
	<div class="mx-auto max-w-7xl">
		<h1 class="mb-8 text-3xl font-bold">Point of Sale</h1>

		{#if isLoading}
			<div class="flex items-center justify-center py-20 text-gray-400">
				<span class="animate-pulse">Loading inventory...</span>
			</div>
		{:else if errorMessage}
			<div class="mb-8 rounded border border-red-500 bg-red-500/10 p-4 text-red-400">
				{errorMessage}
			</div>
		{:else}
			<div class="grid grid-cols-1 gap-8 lg:grid-cols-3">
				<!-- Medicines List -->
				<div class="lg:col-span-2">
					<h2 class="mb-4 text-xl font-semibold">Select Medicines</h2>
					{#if medicines.length == 0}
						<div
							class="rounded-lg border border-gray-700 bg-gray-800 py-20 text-center text-gray-400"
						>
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
											>Ingredient</th
										>
										<th
											class="px-6 py-3 text-left text-xs font-medium tracking-wider text-gray-400 uppercase"
											>Unit</th
										>
										<th
											class="px-6 py-3 text-right text-xs font-medium tracking-wider text-gray-400 uppercase"
											>Action</th
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
													onclick={() => {
														selectedMedicineId = medicine.id;
														selectedQuantity = 1;
													}}
													class="text-blue-400 transition hover:text-blue-300"
												>
													Add
												</button>
											</td>
										</tr>
									{/each}
								</tbody>
							</table>
						</div>
					{/if}
				</div>

				<!-- Cart Sidebar -->
				<div class="lg:col-span-1">
					<div class="sticky top-8 rounded-lg border border-gray-700 bg-gray-800 p-6">
						<h2 class="mb-6 text-xl font-semibold">Shopping Cart</h2>

						<!-- Add to Cart Form -->
						{#if medicines.length > 0}
							<div class="mb-6 border-b border-gray-700 pb-6">
								<label for="medicine-select" class="mb-2 block text-sm font-medium text-gray-300">
									Medicine
								</label>
								<select
									id="medicine-select"
									bind:value={selectedMedicineId}
									class="mb-3 w-full rounded border border-gray-600 bg-gray-700 px-3 py-2 text-sm text-white"
								>
									<option value="">Select a medicine...</option>
									{#each medicines as medicine}
										<option value={medicine.id}>{medicine.name}</option>
									{/each}
								</select>

								<label for="quantity-input" class="mb-2 block text-sm font-medium text-gray-300">
									Quantity
								</label>
								<input
									id="quantity-input"
									type="number"
									min="1"
									bind:value={selectedQuantity}
									class="mb-3 w-full rounded border border-gray-600 bg-gray-700 px-3 py-2 text-sm text-white"
								/>

								<button
									onclick={addToCart}
									class="w-full rounded bg-blue-600 py-2 font-medium text-white transition hover:bg-blue-700"
								>
									Add to Cart
								</button>
							</div>
						{/if}

						<!-- Cart Items -->
						{#if cart.length === 0}
							<div class="py-8 text-center text-gray-400">Cart is empty</div>
						{:else}
							<div class="mb-6 max-h-96 space-y-3 overflow-y-auto">
								{#each cart as item}
									<div class="rounded bg-gray-700 p-3">
										<div class="mb-2 flex items-start justify-between">
											<span class="text-sm font-medium text-white">{item.name}</span>
											<button
												onclick={() => removeFromCart(item.medicineId)}
												class="text-xs text-red-400 hover:text-red-300"
											>
												Remove
											</button>
										</div>
										<div class="flex items-center gap-2">
											<button
												onclick={() => updateQuantity(item.medicineId, item.quantity - 1)}
												class="rounded bg-gray-600 px-2 py-1 text-xs hover:bg-gray-500"
											>
												-
											</button>
											<span class="flex-1 text-center text-sm">{item.quantity}</span>
											<button
												onclick={() => updateQuantity(item.medicineId, item.quantity + 1)}
												class="rounded bg-gray-600 px-2 py-1 text-xs hover:bg-gray-500"
											>
												+
											</button>
										</div>
									</div>
								{/each}
							</div>

							<button
								onclick={checkoutCart}
								disabled={isCheckingOut}
								class="w-full rounded bg-emerald-600 py-2 font-medium text-white transition hover:bg-emerald-700 disabled:bg-gray-600"
							>
								{isCheckingOut ? 'Processing...' : `Checkout (${cart.length} items)`}
							</button>
						{/if}
					</div>
				</div>
			</div>
		{/if}
	</div>
</main>
