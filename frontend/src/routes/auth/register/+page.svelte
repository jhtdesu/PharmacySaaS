<script lang="ts">
	import { goto } from '$app/navigation';
	import { authApi } from '$lib/api';

	type RegisterMode = 'tenant' | 'user';

	let mode = $state<RegisterMode>('user');

	let storeName = $state('');
	let tenantFullName = $state('');
	let fullName = $state('');
	let email = $state('');
	let phoneNumber = $state('');
	let address = $state('');
	let subscription = $state(0);
	let role = $state('User');
	let password = $state('');
	let confirmPassword = $state('');

	let errorMessage = $state('');
	let successMessage = $state('');
	let isLoading = $state(false);

	function setMode(nextMode: RegisterMode) {
		mode = nextMode;
		errorMessage = '';
		successMessage = '';
	}

	async function handleRegister(event: SubmitEvent) {
		event.preventDefault();

		errorMessage = '';
		successMessage = '';

		if (password.length < 8) {
			errorMessage = 'Password must be at least 8 characters.';
			return;
		}

		if (password !== confirmPassword) {
			errorMessage = 'Passwords do not match.';
			return;
		}

		isLoading = true;

		try {
			const response =
				mode === 'tenant'
					? await authApi.post('/tenant/register', {
						storeName,
						address,
						phoneNumber,
						email,
						password,
						tenantFullName,
						subscription
					})
					: await authApi.post('/auth/register', {
						email,
						password,
						fullName,
						role 
					});

			successMessage =
				response.data?.message ||
				(mode === 'tenant' ? 'Tenant registered successfully!' : 'User registered successfully!');

			setTimeout(() => {
				goto('/auth/login');
			}, 1200);
		} catch (error: any) {
			errorMessage = error.response?.data?.message || 'Registration failed. Please try again.';
		} finally {
			isLoading = false;
		}
	}
</script>

<main class="min-h-screen bg-gray-900 flex items-center justify-center p-4">
	<div class="max-w-lg w-full bg-gray-800 rounded-xl shadow-2xl p-8 border border-gray-700">
		<h2 class="text-3xl font-bold text-white text-center mb-2">Create Account</h2>
		<p class="text-gray-400 text-center mb-6">Register a tenant or a normal user account.</p>

		<div class="grid grid-cols-2 gap-2 p-1 bg-gray-700 rounded-lg mb-6">
			<button
				type="button"
				onclick={() => setMode('user')}
				class={`rounded-md px-3 py-2 text-sm font-semibold transition ${mode === 'user' ? 'bg-emerald-500 text-gray-900' : 'text-gray-300 hover:bg-gray-600'}`}
			>
				Normal User
			</button>
			<button
				type="button"
				onclick={() => setMode('tenant')}
				class={`rounded-md px-3 py-2 text-sm font-semibold transition ${mode === 'tenant' ? 'bg-emerald-500 text-gray-900' : 'text-gray-300 hover:bg-gray-600'}`}
			>
				Tenant
			</button>
		</div>

		<form onsubmit={handleRegister} class="space-y-4">
			{#if errorMessage}
				<div class="p-3 bg-red-500/10 border border-red-500 text-red-400 rounded text-sm">
					{errorMessage}
				</div>
			{/if}

			{#if successMessage}
				<div class="p-3 bg-emerald-500/10 border border-emerald-500 text-emerald-300 rounded text-sm">
					{successMessage}
				</div>
			{/if}

			{#if mode === 'tenant'}
				<div>
					<label for="storeName" class="block text-sm font-medium text-gray-400">Store Name</label>
					<input
						id="storeName"
						type="text"
						bind:value={storeName}
						required
						class="mt-1 block w-full rounded-md border-gray-600 bg-gray-700 text-white shadow-sm focus:border-emerald-500 focus:ring-emerald-500 sm:text-sm px-4 py-2"
						placeholder="Greenleaf Pharmacy"
					/>
				</div>

				<div>
					<label for="tenantFullName" class="block text-sm font-medium text-gray-400"
						>Owner Full Name</label
					>
					<input
						id="tenantFullName"
						type="text"
						bind:value={tenantFullName}
						required
						class="mt-1 block w-full rounded-md border-gray-600 bg-gray-700 text-white shadow-sm focus:border-emerald-500 focus:ring-emerald-500 sm:text-sm px-4 py-2"
						placeholder="Jane Doe"
					/>
				</div>
			{:else}
				<div>
					<label for="fullName" class="block text-sm font-medium text-gray-400">Full Name</label>
					<input
						id="fullName"
						type="text"
						bind:value={fullName}
						required
						class="mt-1 block w-full rounded-md border-gray-600 bg-gray-700 text-white shadow-sm focus:border-emerald-500 focus:ring-emerald-500 sm:text-sm px-4 py-2"
						placeholder="John Doe"
					/>
				</div>
			{/if}

			<div class={`grid grid-cols-1 ${mode === 'tenant' ? 'sm:grid-cols-2' : ''} gap-4`}>
				<div>
					<label for="email" class="block text-sm font-medium text-gray-400">Email</label>
					<input
						id="email"
						type="email"
						bind:value={email}
						required
						class="mt-1 block w-full rounded-md border-gray-600 bg-gray-700 text-white shadow-sm focus:border-emerald-500 focus:ring-emerald-500 sm:text-sm px-4 py-2"
						placeholder="owner@pharmacy.com"
					/>
				</div>

				{#if mode === 'tenant'}
					<div>
						<label for="phoneNumber" class="block text-sm font-medium text-gray-400"
							>Phone Number</label
						>
						<input
							id="phoneNumber"
							type="tel"
							bind:value={phoneNumber}
							required
							class="mt-1 block w-full rounded-md border-gray-600 bg-gray-700 text-white shadow-sm focus:border-emerald-500 focus:ring-emerald-500 sm:text-sm px-4 py-2"
							placeholder="+1 555-0100"
						/>
					</div>
				{/if}
			</div>

			{#if mode === 'tenant'}
				<div>
					<label for="address" class="block text-sm font-medium text-gray-400">Address</label>
					<input
						id="address"
						type="text"
						bind:value={address}
						required
						class="mt-1 block w-full rounded-md border-gray-600 bg-gray-700 text-white shadow-sm focus:border-emerald-500 focus:ring-emerald-500 sm:text-sm px-4 py-2"
						placeholder="123 Main St"
					/>
				</div>
			{/if}

			<div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
				<div>
					<label for="password" class="block text-sm font-medium text-gray-400">Password</label>
					<input
						id="password"
						type="password"
						bind:value={password}
						required
						minlength="8"
						class="mt-1 block w-full rounded-md border-gray-600 bg-gray-700 text-white shadow-sm focus:border-emerald-500 focus:ring-emerald-500 sm:text-sm px-4 py-2"
						placeholder="At least 8 characters"
					/>
				</div>

				<div>
					<label for="confirmPassword" class="block text-sm font-medium text-gray-400">Confirm Password</label>
					<input
						id="confirmPassword"
						type="password"
						bind:value={confirmPassword}
						required
						minlength="8"
						class="mt-1 block w-full rounded-md border-gray-600 bg-gray-700 text-white shadow-sm focus:border-emerald-500 focus:ring-emerald-500 sm:text-sm px-4 py-2"
						placeholder="Repeat password"
					/>
				</div>
			</div>

			<button
				type="submit"
				disabled={isLoading}
				class="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-bold text-gray-900 bg-emerald-500 hover:bg-emerald-400 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-emerald-500 focus:ring-offset-gray-900 disabled:opacity-50 transition"
			>
				{isLoading
					? mode === 'tenant'
						? 'Creating tenant...'
						: 'Creating user...'
					: mode === 'tenant'
						? 'Create Tenant Account'
						: 'Create User Account'}
			</button>
		</form>

		<p class="text-sm text-gray-400 mt-5 text-center">
			Already have an account?
			<a href="/auth/login" class="text-emerald-400 hover:text-emerald-300 font-medium">Sign in</a>
		</p>
	</div>
</main>
