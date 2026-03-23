<script lang="ts">
    import { api } from '$lib/api'; 
    import { goto } from '$app/navigation';

    let email = '';
    let password = '';
    let errorMessage = '';
    let isLoading = false;

    async function handleLogin() {
        isLoading = true;
        errorMessage = '';

        try {
            const response = await api.post('/auth/login', {
                email: email,
                password: password
            });

            const token = response.data.accessToken;
            
            localStorage.setItem('jwt_token', token);

            goto('/homepage'); 
        } catch (error: any) {
            errorMessage = error.response?.data?.message || 'Login failed. Check your credentials.';
        } finally {
            isLoading = false;
        }
    }
</script>

<main class="min-h-screen bg-gray-900 flex items-center justify-center p-4">
    <div class="max-w-md w-full bg-gray-800 rounded-xl shadow-2xl p-8 border border-gray-700">
        <h2 class="text-3xl font-bold text-white text-center mb-6">Pharmacy SaaS</h2>
        
        <form on:submit|preventDefault={handleLogin} class="space-y-6">
            
            {#if errorMessage}
                <div class="p-3 bg-red-500/10 border border-red-500 text-red-400 rounded text-sm">
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
                    class="mt-1 block w-full rounded-md border-gray-600 bg-gray-700 text-white shadow-sm focus:border-emerald-500 focus:ring-emerald-500 sm:text-sm px-4 py-2" 
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
                    class="mt-1 block w-full rounded-md border-gray-600 bg-gray-700 text-white shadow-sm focus:border-emerald-500 focus:ring-emerald-500 sm:text-sm px-4 py-2" 
                    placeholder="••••••••"
                />
            </div>

            <button 
                type="submit" 
                disabled={isLoading}
                class="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-bold text-gray-900 bg-emerald-500 hover:bg-emerald-400 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-emerald-500 focus:ring-offset-gray-900 disabled:opacity-50 transition"
            >
                {isLoading ? 'Authenticating...' : 'Sign In'}
            </button>
        </form>
    </div>
</main>