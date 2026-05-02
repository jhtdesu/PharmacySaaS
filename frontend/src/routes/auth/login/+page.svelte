<script lang="ts">
	import { authApi } from '$lib/api';
	import { goto } from '$app/navigation';

	let email = '';
	let password = '';
	let errorMessage = '';
	let isLoading = false;

	async function handleLogin() {
		isLoading = true;
		errorMessage = '';

		try {
			const response = await authApi.post('/auth/login', {
				email: email,
				password: password
			});

			const token = response.data.data.accessToken;
			const refresh = response.data.data.refreshToken;
			const fullName = response.data.data.fullName ?? response.data.data.FullName;

			localStorage.setItem('jwt_token', token);
			localStorage.setItem('refresh_token', refresh);
			if (fullName) {
				localStorage.setItem('user_full_name', fullName);
			}

			// Decode JWT to extract TenantId claim
			try {
				const payload = JSON.parse(atob(token.split('.')[1]));
				const tenantId = payload['TenantId'] || payload['tenantId'];
				if (tenantId) {
					localStorage.setItem('tenant_id', tenantId);
				}
			} catch (e) {
				console.warn('Could not decode JWT payload', e);
			}

			goto('/homepage');
		} catch (error: any) {
			errorMessage = error.response?.data?.message || 'Login failed. Check your credentials.';
		} finally {
			isLoading = false;
		}
	}
</script>

<main class="flex min-h-screen items-center justify-center bg-gray-900 p-4">
	<div class="w-full max-w-md rounded-xl border border-gray-700 bg-gray-800 p-8 shadow-2xl">
		<h2 class="mb-6 text-center text-3xl font-bold text-white">Pharmacy SaaS</h2>

		<form on:submit|preventDefault={handleLogin} class="space-y-6">
			{#if errorMessage}
				<div class="rounded border border-red-500 bg-red-500/10 p-3 text-sm text-red-400">
					{errorMessage}
				</div>
			{/if}

			<div>
				<label for="email" class="block text-sm font-medium text-gray-400">Email Address</label>
				<input
					type="email"
					id="email"
					bind:value={email}
					required
					class="mt-1 block w-full rounded-md border-gray-600 bg-gray-700 px-4 py-2 text-white shadow-sm focus:border-emerald-500 focus:ring-emerald-500 sm:text-sm"
					placeholder="pharmacist@example.com"
				/>
			</div>

			<div>
				<label for="password" class="block text-sm font-medium text-gray-400">Password</label>
				<input
					type="password"
					id="password"
					bind:value={password}
					required
					class="mt-1 block w-full rounded-md border-gray-600 bg-gray-700 px-4 py-2 text-white shadow-sm focus:border-emerald-500 focus:ring-emerald-500 sm:text-sm"
					placeholder="••••••••"
				/>
			</div>

			<button
				type="submit"
				disabled={isLoading}
				class="flex w-full justify-center rounded-md border border-transparent bg-emerald-500 px-4 py-2 text-sm font-bold text-gray-900 shadow-sm transition hover:bg-emerald-400 focus:ring-2 focus:ring-emerald-500 focus:ring-offset-2 focus:ring-offset-gray-900 focus:outline-none disabled:opacity-50"
			>
				{isLoading ? 'Authenticating...' : 'Sign In'}
			</button>
		</form>
	</div>
</main>
