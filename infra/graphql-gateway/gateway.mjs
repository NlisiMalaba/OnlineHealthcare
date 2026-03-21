import { ApolloServer } from "@apollo/server";
import { startStandaloneServer } from "@apollo/server/standalone";
import { stitchSchemas } from "@graphql-tools/stitch";
import { makeExecutableSchema } from "@graphql-tools/schema";
import { readFileSync, readdirSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = dirname(fileURLToPath(import.meta.url));
const stubsDir = join(__dirname, "stubs");

function loadSubschemas() {
  const files = readdirSync(stubsDir).filter((f) => f.endsWith(".graphql"));
  return files.map((file) => {
    const name = file.replace(/\.graphql$/, "");
    const typeDefs = readFileSync(join(stubsDir, file), "utf8");
    const match = typeDefs.match(/type\s+Query\s*\{\s*(\w+)\s*:/);
    if (!match) {
      throw new Error(`Stub ${file} must declare a single Query field`);
    }
    const field = match[1];
    return {
      schema: makeExecutableSchema({
        typeDefs,
        resolvers: {
          Query: {
            [field]: () => `stub:${name}`,
          },
        },
      }),
    };
  });
}

const gatewaySchema = stitchSchemas({
  subschemas: loadSubschemas(),
});

const server = new ApolloServer({ schema: gatewaySchema });
const port = Number(process.env.PORT || 4000);
const { url } = await startStandaloneServer(server, {
  listen: { host: "0.0.0.0", port },
});
console.log(`GraphQL gateway ready at ${url}`);
