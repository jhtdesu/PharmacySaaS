<script lang="ts">
	import { page } from '$app/stores';
	import { onMount } from 'svelte';

	let orderId = $state('');
	let status = $state<'loading' | 'success' | 'error'>('loading');
	let message = $state('Processing payment...');
	let receiptNumber = $state('');

	onMount(async () => {
		orderId = $page.params.orderId;

		if (!orderId) {
			status = 'error';
			message = 'No order ID provided.';
			return;
		}

		try {
			status = 'loading';
			message = 'Confirming payment completion...';

			// Poll for sale status with retries
			let maxAttempts = 10;
			let attempt = 0;
			let saleData: any = null;

			while (attempt < maxAttempts && !saleData) {
				await new Promise(resolve => setTimeout(resolve, 1000));

				try {
					const response = await fetch(
						`https://api.jhtdesu-app.tech/api/sales/${orderId}`,
						{ method: 'GET' }
					);

					if (response.ok) {
						const data = await response.json();
						if (data.data) {
							saleData = data.data;
							break;
						}
					}
				} catch (pollError) {
					// Continue polling on error
				}

				attempt++;
			}

			if (saleData) {
				status = 'success';
				message = 'Payment completed successfully!';
				receiptNumber = saleData.receiptNumber || `RCPT-${Date.now().toString().slice(-10)}`;
			} else {
				status = 'error';
				message = 'Payment processing timed out. Please check your order status.';
			}
		} catch (error) {
			status = 'error';
			message = error instanceof Error ? error.message : 'Unable to process payment.';
		}
	});
</script>

<main class="min-h-screen bg-gray-900 text-white flex items-center justify-center p-6">
	<div class="w-full max-w-lg rounded-xl border border-gray-700 bg-gray-800 p-8">
		<div class="text-center">
			<h1 class="text-3xl font-bold mb-6">
				{#if status === 'loading'}
					Payment Processing...
				{:else if status === 'success'}
					✓ Payment Successful
				{:else}
					✗ Payment Failed
				{/if}
			</h1>

			<p
				class={`text-base mb-6 ${
					status === 'loading'
						? 'text-gray-300'
						: status === 'success'
						? 'text-green-400'
						: 'text-red-400'
				}`}
			>
				{message}
			</p>

			{#if status === 'success'}
				<div class="bg-gray-700 rounded-lg p-4 mb-6 text-left">
					<div class="mb-3">
						<p class="text-gray-400 text-sm">Order ID</p>
						<p class="text-white font-mono break-all">{orderId}</p>
					</div>
					{#if receiptNumber}
						<div>
							<p class="text-gray-400 text-sm">Receipt Number</p>
							<p class="text-white font-mono">{receiptNumber}</p>
						</div>
					{/if}
				</div>

				<a
					href="/dashboard"
					class="inline-block px-6 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-semibold transition"
				>
					Return to Dashboard
				</a>
			{:else if status === 'error'}
				<a
					href="/"
					class="inline-block px-6 py-2 bg-gray-600 hover:bg-gray-700 text-white rounded-lg font-semibold transition"
				>
					Back to Home
				</a>
			{:else}
				<p class="text-gray-400 text-sm">Please wait...</p>
			{/if}
		</div>
	</div>
</main>
