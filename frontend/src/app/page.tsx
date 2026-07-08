export default function Home() {
  return (
    <main className="flex min-h-screen items-center justify-center bg-background px-6 py-12 text-foreground">
      <section className="w-full max-w-2xl">
        <p className="text-sm font-medium text-muted-foreground">
          Education Center CRM
        </p>
        <h1 className="mt-3 text-3xl font-semibold tracking-normal sm:text-4xl">
          Nền tảng frontend đã sẵn sàng.
        </h1>
        <p className="mt-4 max-w-xl text-base leading-7 text-muted-foreground">
          Next.js, TypeScript, Tailwind CSS, Axios Client và React Query đã được
          cấu hình để triển khai các màn hình tiếp theo.
        </p>
      </section>
    </main>
  );
}
