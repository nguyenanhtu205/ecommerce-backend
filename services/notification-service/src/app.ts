import express, {Express} from 'express';
import cors from 'cors';
import helmet from 'helmet';
import {routes} from './routes';

export function createApp(): Express {
  const app = express();

  app.use(helmet());
  app.use(cors());
  app.use(express.json());

  app.use(routes);

  return app;
}
