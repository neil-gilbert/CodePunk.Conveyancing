import type { NextAuthOptions } from 'next-auth'
import Auth0Provider from 'next-auth/providers/auth0'

export const authOptions: NextAuthOptions = {
  providers: [
    Auth0Provider({
      clientId: process.env.AUTH0_CLIENT_ID!,
      clientSecret: process.env.AUTH0_CLIENT_SECRET!,
      issuer: process.env.AUTH0_ISSUER,
    }),
  ],
  session: { strategy: 'jwt' },
  callbacks: {
    async jwt({ token, account }) {
      if (account) token.provider = account.provider
      return token
    },
    async session({ session, token }) {
      (session as any).provider = token.provider
      return session
    },
  },
}

