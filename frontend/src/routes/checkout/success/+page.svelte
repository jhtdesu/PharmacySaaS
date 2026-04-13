<script lang="ts">
	import { onMount } from 'svelte';
	import { authApi } from '$lib/api';

	let status = $state<'loading' | 'success' | 'failed'>('loading');
	let message = $state('Processing payment result...');

	onMount(async () => {
		const searchParams = new URLSearchParams(window.location.search);

		const hasMomoPayload = searchParams.has('partnerCode') && searchParams.has('orderId') && searchParams.has('signature');
		if (!hasMomoPayload) {
			status = 'failed';
			message = 'Invalid payment callback data.';
			return;
		}

		try {
			await authApi.get(`momo/webhook?${searchParams.toString()}`);

			const resultCode = Number(searchParams.get('resultCode') ?? '-1');
			if (resultCode === 0) {
				status = 'success';
				message = 'Payment completed successfully.';
			} else {
				status = 'failed';
				message = decodeURIComponent(searchParams.get('message') ?? 'Payment was not successful.');
			}
		} catch {
			status = 'failed';
			message = 'Unable to process payment callback.';
		}
	});
</script>

<main class="min-h-screen bg-gray-900 text-white flex items-center justify-center p-6">
	<div class="w-full max-w-lg rounded-xl border border-gray-700 bg-gray-800 p-8 text-center">
		<h1 class="text-2xl font-bold mb-4">MoMo Payment Result</h1>
		<p
			class={`text-sm ${
				status === 'loading'
					? 'text-gray-300'
					: status === 'success'
					? 'text-green-400'
					: 'text-red-400'
			}`}
		>
			{message}
		</p>
	</div>
</main>